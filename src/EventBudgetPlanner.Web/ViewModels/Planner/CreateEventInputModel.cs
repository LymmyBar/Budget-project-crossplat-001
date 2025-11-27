using System.ComponentModel.DataAnnotations;

namespace EventBudgetPlanner.Web.ViewModels.Planner;

public sealed class CreateEventInputModel
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    [Display(Name = "Event name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Event date")]
    public DateOnly? Date { get; set; }

    [Required]
    [StringLength(120)]
    public string Venue { get; set; } = string.Empty;

    [Required]
    [Range(1, 1_000_000_000)]
    [Display(Name = "Target budget")]
    public decimal TargetBudget { get; set; }

    [Required]
    [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Use a three-letter ISO currency code (e.g. USD).")]
    public string Currency { get; set; } = "USD";
}
