using EventBudgetPlanner.Core.Abstractions;
using EventBudgetPlanner.Core.Models;
using EventBudgetPlanner.Core.Persistence;
using EventBudgetPlanner.Core.Services;

namespace EventBudgetPlanner.Tests;

public sealed class EventPlannerServiceTests
{
    [Fact]
    public async Task CreateEventAsync_PersistsToStore()
    {
        var store = new InMemoryDataStore();
        var guidProvider = new DeterministicGuidProvider(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var service = new EventPlannerService(store, guidProvider);

        var plan = await service.CreateEventAsync("Tech Expo", new DateOnly(2025, 4, 2), "Kyiv", 12_000m, "USD");

        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), plan.Id);
        var stored = await store.LoadAsync();
        Assert.Single(stored);
        Assert.Equal(plan.Name, stored[0].Name);
    }

    [Fact]
    public async Task Summary_ComputesRemainingBudgetAndFlagsOverrun()
    {
        var store = new InMemoryDataStore();
        var guidProvider = new DeterministicGuidProvider(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"));

        var service = new EventPlannerService(store, guidProvider);
        var plan = await service.CreateEventAsync("Music Fest", new DateOnly(2025, 6, 15), "Lviv", 5_000m, "EUR");
        await service.AddBudgetItemAsync(plan.Id, "Stage", "Stage rental", 4_200m, BudgetStatus.Paid);
        await service.AddStaffAssignmentAsync(plan.Id, "Anna Kovalenko", "Coordinator", 50m, 24);

        var summary = await service.GetEventSummaryAsync(plan.Id) ?? throw new InvalidOperationException();

        Assert.Equal(4_200m, summary.TotalPaid);
        Assert.Equal(1_200m, summary.StaffCost);
        Assert.True(summary.IsOverBudget);
        Assert.Equal(-400m, summary.RemainingBudget);
    }

    [Fact]
    public async Task PortfolioSummary_AggregatesTotals()
    {
        var store = new InMemoryDataStore();
        var guidProvider = new DeterministicGuidProvider(Guid.NewGuid());
        var service = new EventPlannerService(store, guidProvider);

        var eventA = await service.CreateEventAsync("Conference", new DateOnly(2025, 1, 10), "Kyiv", 10_000m, "USD");
        await service.AddBudgetItemAsync(eventA.Id, "Venue", "Hall", 3_000m, BudgetStatus.Paid);
        await service.AddStaffAssignmentAsync(eventA.Id, "Oleh", "PM", 60m, 30);

        var eventB = await service.CreateEventAsync("Workshop", new DateOnly(2025, 2, 5), "Odesa", 6_000m, "USD");
        await service.AddBudgetItemAsync(eventB.Id, "Marketing", "Ads", 1_500m, BudgetStatus.Committed);

        var events = await service.ListEventsAsync();
        var portfolio = service.BuildPortfolioSummary(events);

        Assert.Equal(2, portfolio.EventCount);
        Assert.Equal(16_000m, portfolio.TotalTargetBudget);
        Assert.Equal(3_000m, portfolio.TotalPaid);
        Assert.Equal(1_500m, portfolio.TotalCommitted);
        Assert.Equal(1_800m, portfolio.TotalStaffCost);
    }

    [Fact]
    public async Task JsonFileDataStore_RoundTripsEvents()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"planner-{Guid.NewGuid():N}.json");
        try
        {
            var store = new JsonFileDataStore(tempFile);
            var events = new List<EventPlan>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample",
                    Date = new DateOnly(2025, 5, 5),
                    Venue = "Dnipro",
                    TargetBudget = 1000m,
                    Currency = "USD",
                    BudgetItems = new List<BudgetItem>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Category = "Venue",
                            Amount = 500m,
                            Status = BudgetStatus.Planned
                        }
                    }
                }
            };

            await store.SaveAsync(events);
            var loaded = await store.LoadAsync();

            Assert.Single(loaded);
            Assert.Equal(events[0].Name, loaded[0].Name);
            Assert.Equal(events[0].BudgetItems.Count, loaded[0].BudgetItems.Count);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}

file sealed class InMemoryDataStore : IDataStore
{
    private readonly List<EventPlan> _items = new();

    public Task<IReadOnlyList<EventPlan>> LoadAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EventPlan> snapshot = _items.Select(Clone).ToList();
        return Task.FromResult(snapshot);
    }

    public Task SaveAsync(IReadOnlyCollection<EventPlan> events, CancellationToken cancellationToken = default)
    {
        _items.Clear();
        _items.AddRange(events.Select(Clone));
        return Task.CompletedTask;
    }

    private static EventPlan Clone(EventPlan source) => new()
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

file sealed class DeterministicGuidProvider : IGuidProvider
{
    private readonly Queue<Guid> _values;

    public DeterministicGuidProvider(params Guid[] values)
    {
        if (values is null || values.Length == 0)
        {
            throw new ArgumentException("At least one GUID is required", nameof(values));
        }

        _values = new Queue<Guid>(values);
    }

    public Guid Create()
    {
        if (_values.Count == 0)
        {
            return Guid.NewGuid();
        }

        return _values.Dequeue();
    }
}
