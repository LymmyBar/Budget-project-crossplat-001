using EventBudgetPlanner.Core.Models;

namespace EventBudgetPlanner.Web.ViewModels.Planner;

public sealed class DashboardViewModel
{
    public required IReadOnlyList<EventCardViewModel> Events { get; init; }

    public required PortfolioSummary Portfolio { get; init; }

    public bool HasEvents => Events.Count > 0;
}
