using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EventBudgetPlanner.Web.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(500)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;
}