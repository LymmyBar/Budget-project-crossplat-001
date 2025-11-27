using System.Linq;
using EventBudgetPlanner.Web.Models;
using EventBudgetPlanner.Web.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventBudgetPlanner.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var externalLogins = await _userManager.GetLoginsAsync(user);

        var model = new ProfileViewModel
        {
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = string.IsNullOrWhiteSpace(user.PhoneNumber) ? "Not provided" : user.PhoneNumber,
            ExternalLoginProviders = externalLogins.Select(login => login.LoginProvider).Distinct().ToList()
        };

        return View(model);
    }
}
