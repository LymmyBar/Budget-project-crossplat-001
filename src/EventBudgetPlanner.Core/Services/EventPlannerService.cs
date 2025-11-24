using EventBudgetPlanner.Core.Abstractions;
using EventBudgetPlanner.Core.Models;
using EventBudgetPlanner.Core.Persistence;

namespace EventBudgetPlanner.Core.Services;

public sealed class EventPlannerService
{
    private readonly IDataStore _dataStore;
    private readonly IGuidProvider _guidProvider;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private List<EventPlan> _cache = new();
    private bool _initialized;

    public EventPlannerService(IDataStore dataStore, IGuidProvider guidProvider)
    {
        _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        _guidProvider = guidProvider ?? throw new ArgumentNullException(nameof(guidProvider));
    }

    public async Task<EventPlan> CreateEventAsync(string name, DateOnly date, string venue, decimal targetBudget, string currency, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required", nameof(name));
        }

        if (targetBudget <= 0)
        {
            throw new ArgumentException("Budget must be positive", nameof(targetBudget));
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

            var plan = new EventPlan
            {
                Id = _guidProvider.Create(),
                Name = name.Trim(),
                Date = date,
                Venue = venue?.Trim() ?? string.Empty,
                TargetBudget = decimal.Round(targetBudget, 2),
                Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant()
            };

            _cache.Add(plan);
            await PersistAsync(cancellationToken).ConfigureAwait(false);
            return plan;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<EventPlan>> ListEventsAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            return _cache.Select(CloneEvent).ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<EventPlan?> GetEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            var plan = _cache.FirstOrDefault(e => e.Id == eventId);
            return plan is null ? null : CloneEvent(plan);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<BudgetItem> AddBudgetItemAsync(Guid eventId, string category, string description, decimal amount, BudgetStatus status, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be positive", nameof(amount));
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            var plan = RequireEvent(eventId);
            var item = new BudgetItem
            {
                Id = _guidProvider.Create(),
                Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim(),
                Description = description?.Trim() ?? string.Empty,
                Amount = decimal.Round(amount, 2),
                Status = status
            };

            plan.BudgetItems.Add(item);
            await PersistAsync(cancellationToken).ConfigureAwait(false);
            return item;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<StaffAssignment> AddStaffAssignmentAsync(Guid eventId, string fullName, string role, decimal hourlyRate, double hoursBooked, CancellationToken cancellationToken = default)
    {
        if (hourlyRate <= 0)
        {
            throw new ArgumentException("Hourly rate must be positive", nameof(hourlyRate));
        }

        if (hoursBooked <= 0)
        {
            throw new ArgumentException("Hours must be positive", nameof(hoursBooked));
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            var plan = RequireEvent(eventId);
            var assignment = new StaffAssignment
            {
                Id = _guidProvider.Create(),
                FullName = string.IsNullOrWhiteSpace(fullName) ? "Unassigned" : fullName.Trim(),
                Role = role?.Trim() ?? string.Empty,
                HourlyRate = decimal.Round(hourlyRate, 2),
                HoursBooked = Math.Round(hoursBooked, 2)
            };

            plan.Staff.Add(assignment);
            await PersistAsync(cancellationToken).ConfigureAwait(false);
            return assignment;
        }
        finally
        {
            _gate.Release();
        }
    }

    public EventSummary BuildEventSummary(EventPlan plan)
    {
        if (plan is null)
        {
            throw new ArgumentNullException(nameof(plan));
        }

        var totals = new
        {
            planned = Sum(plan.BudgetItems, BudgetStatus.Planned),
            committed = Sum(plan.BudgetItems, BudgetStatus.Committed),
            paid = Sum(plan.BudgetItems, BudgetStatus.Paid)
        };

        var staffCost = plan.Staff.Sum(s => s.TotalCost);
        var spent = totals.paid + staffCost;
        var remaining = plan.TargetBudget - spent;

        return new EventSummary
        {
            EventId = plan.Id,
            Name = plan.Name,
            TargetBudget = plan.TargetBudget,
            TotalPlanned = totals.planned,
            TotalCommitted = totals.committed,
            TotalPaid = totals.paid,
            StaffCost = staffCost,
            RemainingBudget = remaining,
            IsOverBudget = remaining < 0
        };
    }

    public PortfolioSummary BuildPortfolioSummary(IEnumerable<EventPlan> events)
    {
        var materialized = events?.ToList() ?? throw new ArgumentNullException(nameof(events));

        var summaries = materialized.Select(BuildEventSummary).ToList();

        return new PortfolioSummary
        {
            EventCount = materialized.Count,
            TotalTargetBudget = materialized.Sum(e => e.TargetBudget),
            TotalPlanned = summaries.Sum(s => s.TotalPlanned),
            TotalCommitted = summaries.Sum(s => s.TotalCommitted),
            TotalPaid = summaries.Sum(s => s.TotalPaid),
            TotalStaffCost = summaries.Sum(s => s.StaffCost),
            OverBudgetEvents = summaries.Count(s => s.IsOverBudget)
        };
    }

    public async Task<EventSummary?> GetEventSummaryAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var plan = await GetEventAsync(eventId, cancellationToken).ConfigureAwait(false);
        return plan is null ? null : BuildEventSummary(plan);
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        var events = await _dataStore.LoadAsync(cancellationToken).ConfigureAwait(false);
        _cache = events.ToList();
        _initialized = true;
    }

    private async Task PersistAsync(CancellationToken cancellationToken)
    {
        await _dataStore.SaveAsync(_cache, cancellationToken).ConfigureAwait(false);
    }

    private EventPlan RequireEvent(Guid eventId)
    {
        var plan = _cache.FirstOrDefault(e => e.Id == eventId);
        if (plan is null)
        {
            throw new InvalidOperationException($"Event {eventId} was not found.");
        }

        return plan;
    }

    private static decimal Sum(IEnumerable<BudgetItem> items, BudgetStatus status) =>
        items.Where(i => i.Status == status).Sum(i => i.Amount);

    private static EventPlan CloneEvent(EventPlan source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        Date = source.Date,
        Venue = source.Venue,
        TargetBudget = source.TargetBudget,
        Currency = source.Currency,
        BudgetItems = source.BudgetItems.Select(item => new BudgetItem
        {
            Id = item.Id,
            Category = item.Category,
            Description = item.Description,
            Amount = item.Amount,
            Status = item.Status
        }).ToList(),
        Staff = source.Staff.Select(staff => new StaffAssignment
        {
            Id = staff.Id,
            FullName = staff.FullName,
            Role = staff.Role,
            HourlyRate = staff.HourlyRate,
            HoursBooked = staff.HoursBooked
        }).ToList()
    };
}
