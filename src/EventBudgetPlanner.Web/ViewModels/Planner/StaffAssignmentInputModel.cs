using System.ComponentModel.DataAnnotations;

namespace EventBudgetPlanner.Web.ViewModels.Planner;

public sealed class StaffAssignmentInputModel
{
    [Required]
    [Display(Name = "Event")]
    public Guid? EventId { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Team member")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Role { get; set; } = string.Empty;

    [Required]
    [Range(1, 5_000)]
    [Display(Name = "Hourly rate")]
    public decimal HourlyRate { get; set; }

    [Required]
    [Range(0.5, 500)]
    [Display(Name = "Hours booked")]
    public double HoursBooked { get; set; }
}
