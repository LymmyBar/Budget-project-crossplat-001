using EventBudgetPlanner.Core.Models;

namespace EventBudgetPlanner.Web.ViewModels.Home;

public sealed class LandingPageViewModel
{
    public required int TotalEvents { get; init; }

    public required decimal TotalBudget { get; init; }

    public required int StaffAssignments { get; init; }

    public required int OverBudgetEvents { get; init; }

    public EventPlan? HighlightedEvent { get; init; }
}
