using System.Diagnostics;
using System.Linq;
using EventBudgetPlanner.Core.Services;
using EventBudgetPlanner.Web.Models;
using EventBudgetPlanner.Web.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;

namespace EventBudgetPlanner.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly EventPlannerService _plannerService;

    public HomeController(ILogger<HomeController> logger, EventPlannerService plannerService)
    {
        _logger = logger;
        _plannerService = plannerService;
    }

    public async Task<IActionResult> Index()
    {
        var events = await _plannerService.ListEventsAsync();
        var portfolio = _plannerService.BuildPortfolioSummary(events);
        var highlighted = events
            .OrderBy(e => e.Date)
            .FirstOrDefault(e => e.Date >= DateOnly.FromDateTime(DateTime.Today))
            ?? events.OrderByDescending(e => e.Date).FirstOrDefault();

        var model = new LandingPageViewModel
        {
            TotalEvents = events.Count,
            TotalBudget = portfolio.TotalTargetBudget,
            StaffAssignments = events.Sum(e => e.Staff.Count),
            OverBudgetEvents = portfolio.OverBudgetEvents,
            HighlightedEvent = highlighted
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
