using System.Linq;
using EventBudgetPlanner.Core.Models;
using EventBudgetPlanner.Core.Services;
using EventBudgetPlanner.Web.ViewModels.Planner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventBudgetPlanner.Web.Controllers;

[Authorize]
public class PlannerController : Controller
{
    private readonly EventPlannerService _plannerService;
    private readonly ILogger<PlannerController> _logger;

    public PlannerController(EventPlannerService plannerService, ILogger<PlannerController> logger)
    {
        _plannerService = plannerService;
        _logger = logger;
    }

    public async Task<IActionResult> Dashboard()
    {
        var events = await _plannerService.ListEventsAsync();
        var cards = events
            .OrderBy(e => e.Date)
            .Select(plan => new EventCardViewModel
            {
                Id = plan.Id,
                Name = plan.Name,
                Date = plan.Date,
                Venue = plan.Venue,
                TargetBudget = plan.TargetBudget,
                Currency = plan.Currency,
                Summary = _plannerService.BuildEventSummary(plan)
            })
            .ToList();

        var model = new DashboardViewModel
        {
            Events = cards,
            Portfolio = _plannerService.BuildPortfolioSummary(events)
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult CreateEvent()
    {
        return View(new CreateEventInputModel
        {
            Date = DateOnly.FromDateTime(DateTime.Today)
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent(CreateEventInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        try
        {
            await _plannerService.CreateEventAsync(
                input.Name,
                input.Date!.Value,
                input.Venue,
                input.TargetBudget,
                input.Currency);

            TempData["StatusMessage"] = $"Event '{input.Name}' was created.";
            return RedirectToAction(nameof(Dashboard));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create event");
            ModelState.AddModelError(string.Empty, "Unable to create event. Please try again.");
            return View(input);
        }
    }

    [HttpGet]
    public async Task<IActionResult> AddBudgetItem()
    {
        var eventOptions = await BuildEventOptionsAsync();
        if (eventOptions.Count == 0)
        {
            TempData["StatusMessage"] = "Create an event before adding budget items.";
            return RedirectToAction(nameof(CreateEvent));
        }

        var viewModel = new BudgetItemPageViewModel
        {
            Input = new BudgetItemInputModel(),
            EventOptions = eventOptions,
            StatusOptions = BuildStatusOptions()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AddBudgetItem(BudgetItemInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return View(new BudgetItemPageViewModel
            {
                Input = input,
                EventOptions = await BuildEventOptionsAsync(input.EventId),
                StatusOptions = BuildStatusOptions()
            });
        }

        try
        {
            await _plannerService.AddBudgetItemAsync(
                input.EventId!.Value,
                input.Category,
                input.Description ?? string.Empty,
                input.Amount,
                input.Status);

            TempData["StatusMessage"] = "Budget line item captured.";
            return RedirectToAction(nameof(Dashboard));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add budget item");
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(new BudgetItemPageViewModel
            {
                Input = input,
                EventOptions = await BuildEventOptionsAsync(input.EventId),
                StatusOptions = BuildStatusOptions()
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> AssignStaff()
    {
        var eventOptions = await BuildEventOptionsAsync();
        if (eventOptions.Count == 0)
        {
            TempData["StatusMessage"] = "Create an event before assigning staff.";
            return RedirectToAction(nameof(CreateEvent));
        }

        var viewModel = new StaffAssignmentPageViewModel
        {
            Input = new StaffAssignmentInputModel(),
            EventOptions = eventOptions
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AssignStaff(StaffAssignmentInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return View(new StaffAssignmentPageViewModel
            {
                Input = input,
                EventOptions = await BuildEventOptionsAsync(input.EventId)
            });
        }

        try
        {
            await _plannerService.AddStaffAssignmentAsync(
                input.EventId!.Value,
                input.FullName,
                input.Role,
                input.HourlyRate,
                input.HoursBooked);

            TempData["StatusMessage"] = "Team member assignment saved.";
            return RedirectToAction(nameof(Dashboard));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add staff assignment");
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(new StaffAssignmentPageViewModel
            {
                Input = input,
                EventOptions = await BuildEventOptionsAsync(input.EventId)
            });
        }
    }

    private async Task<IReadOnlyList<SelectListItem>> BuildEventOptionsAsync(Guid? selectedId = null)
    {
        var events = await _plannerService.ListEventsAsync();
        return events
            .OrderBy(e => e.Name)
            .Select(plan => new SelectListItem
            {
                Text = $"{plan.Name} — {plan.Date:MMM d, yyyy}",
                Value = plan.Id.ToString(),
                Selected = selectedId.HasValue && plan.Id == selectedId.Value
            })
            .ToList();
    }

    private static IReadOnlyList<SelectListItem> BuildStatusOptions()
    {
        return Enum.GetValues<BudgetStatus>()
            .Select(status => new SelectListItem(status.ToString(), status.ToString()))
            .ToList();
    }
}
