using System.ComponentModel.DataAnnotations;
using EventBudgetPlanner.Core.Models;

namespace EventBudgetPlanner.Web.ViewModels.Planner;

public sealed class BudgetItemInputModel
{
    [Required]
    [Display(Name = "Event")]
    public Guid? EventId { get; set; }

    [Required]
    [StringLength(60)]
    public string Category { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; set; }

    [Required]
    [Range(1, 1_000_000_000)]
    public decimal Amount { get; set; }

    [Required]
    [Display(Name = "Budget status")]
    public BudgetStatus Status { get; set; } = BudgetStatus.Planned;
}
