using System;
using System.Collections.Generic;
using System.Linq;
using EventBudgetPlanner.Web.Data;
using EventBudgetPlanner.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenIddict.Abstractions;

namespace EventBudgetPlanner.Web.Services;

public class SeedService : ISeedService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    private const string ClientConfigurationSection = "OAuth:EventPlannerWeb";

    public SeedService(
        ApplicationDbContext dbContext,
        IOpenIddictApplicationManager applicationManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _applicationManager = applicationManager;
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        await _dbContext.Database.MigrateAsync();
        await EnsureOAuthClientAsync();
        await EnsureDemoUserAsync();
    }

    private async Task EnsureOAuthClientAsync()
    {
        var clientId = _configuration[$"{ClientConfigurationSection}:ClientId"] ?? "eventplanner-web-client";
        var displayName = _configuration[$"{ClientConfigurationSection}:DisplayName"] ?? "Event Budget Planner";

        var client = await _applicationManager.FindByClientIdAsync(clientId);
        if (client is not null)
        {
            return;
        }

        var redirectUris = ReadUris("RedirectUris", "https://localhost:7268/signin-oidc");
        var postLogoutUris = ReadUris("PostLogoutRedirectUris", "https://localhost:7268/signout-callback-oidc");

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            DisplayName = displayName,
            ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
            ClientType = OpenIddictConstants.ClientTypes.Public,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.Logout,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Address,
                OpenIddictConstants.Permissions.Scopes.Phone,
                OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OpenId
            }
        };

        foreach (var uri in redirectUris)
        {
            descriptor.RedirectUris.Add(uri);
        }

        foreach (var uri in postLogoutUris)
        {
            descriptor.PostLogoutRedirectUris.Add(uri);
        }

        await _applicationManager.CreateAsync(descriptor);
    }

    private async Task EnsureDemoUserAsync()
    {
        const string defaultEmail = "demo@eventplanner.local";
        const string defaultPassword = "DemoUser1!";

        if (await _userManager.Users.AnyAsync(u => u.Email == defaultEmail))
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = defaultEmail,
            Email = defaultEmail,
            EmailConfirmed = true,
            FullName = "Demo User"
        };

        await _userManager.CreateAsync(user, defaultPassword);
    }

    private IReadOnlyList<Uri> ReadUris(string key, string defaultValue)
    {
        var section = _configuration.GetSection($"{ClientConfigurationSection}:{key}");
        var configured = section.Get<string[]>() ?? Array.Empty<string>();
        var uris = new List<Uri>();
        foreach (var value in configured)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (Uri.TryCreate(value, UriKind.Absolute, out var parsed))
            {
                uris.Add(parsed);
            }
        }

        if (uris.Count == 0 && !string.IsNullOrWhiteSpace(defaultValue))
        {
            if (Uri.TryCreate(defaultValue, UriKind.Absolute, out var fallback))
            {
                uris.Add(fallback);
            }
        }

        return uris;
    }
}
