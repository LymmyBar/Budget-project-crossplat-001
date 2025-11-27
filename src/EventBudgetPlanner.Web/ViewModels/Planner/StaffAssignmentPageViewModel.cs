using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventBudgetPlanner.Web.ViewModels.Planner;

public sealed class StaffAssignmentPageViewModel
{
    public required StaffAssignmentInputModel Input { get; init; }

    public required IReadOnlyList<SelectListItem> EventOptions { get; init; }
}
