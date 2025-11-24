namespace EventBudgetPlanner.Core.Models;

public sealed class EventPlan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string Venue { get; set; } = string.Empty;
    public decimal TargetBudget { get; set; }
    public string Currency { get; set; } = "USD";
    public List<BudgetItem> BudgetItems { get; set; } = new();
    public List<StaffAssignment> Staff { get; set; } = new();
}
