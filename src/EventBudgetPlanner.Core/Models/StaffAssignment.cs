namespace EventBudgetPlanner.Core.Models;

public sealed class StaffAssignment
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public double HoursBooked { get; set; }

    public decimal TotalCost => HourlyRate * (decimal)HoursBooked;
}
