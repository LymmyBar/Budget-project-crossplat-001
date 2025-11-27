namespace EventBudgetPlanner.Web.ViewModels.Profile;

public sealed class ProfileViewModel
{
    public required string UserName { get; init; }

    public required string Email { get; init; }

    public required string FullName { get; init; }

    public required string PhoneNumber { get; init; }

    public required IReadOnlyList<string> ExternalLoginProviders { get; init; }

    public bool HasExternalProviders => ExternalLoginProviders.Count > 0;
}
