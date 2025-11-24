namespace EventBudgetPlanner.Core.Models;

public sealed class PortfolioSummary
{
    public int EventCount { get; init; }
    public decimal TotalTargetBudget { get; init; }
    public decimal TotalPlanned { get; init; }
    public decimal TotalCommitted { get; init; }
    public decimal TotalPaid { get; init; }
    public decimal TotalStaffCost { get; init; }
    public int OverBudgetEvents { get; init; }
}
