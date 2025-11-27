using EventBudgetPlanner.Core.Models;

namespace EventBudgetPlanner.Web.ViewModels.Planner;

public sealed class EventCardViewModel
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required DateOnly Date { get; init; }

    public required string Venue { get; init; }

    public required decimal TargetBudget { get; init; }

    public required string Currency { get; init; }

    public required EventSummary Summary { get; init; }
}
