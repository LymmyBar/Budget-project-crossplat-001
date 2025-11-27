using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventBudgetPlanner.Web.ViewModels.Planner;

public sealed class BudgetItemPageViewModel
{
    public required BudgetItemInputModel Input { get; init; }

    public required IReadOnlyList<SelectListItem> EventOptions { get; init; }

    public required IReadOnlyList<SelectListItem> StatusOptions { get; init; }
}
