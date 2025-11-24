namespace EventBudgetPlanner.Core.Models;

public sealed class EventSummary
{
    public Guid EventId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal TargetBudget { get; init; }
    public decimal TotalPlanned { get; init; }
    public decimal TotalCommitted { get; init; }
    public decimal TotalPaid { get; init; }
    public decimal StaffCost { get; init; }
    public decimal RemainingBudget { get; init; }
    public bool IsOverBudget { get; init; }
}
