namespace EventBudgetPlanner.Core.Models;

public sealed class BudgetItem
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public BudgetStatus Status { get; set; } = BudgetStatus.Planned;
}
