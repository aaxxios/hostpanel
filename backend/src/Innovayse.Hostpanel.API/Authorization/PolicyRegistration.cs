using Innovayse.Hostpanel.API.Extensions;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Shared.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.API.Authorization;

/// <summary>
/// Registers the authorization policies every controller authorizes against, and — under SSO —
/// the default policy every scheme-less <c>[Authorize]</c> falls back to.
/// </summary>
public static class PolicyRegistration
{
    /// <summary>Adds hostpanel's authorization policies.</summary>
    /// <param name="services">The container.</param>
    /// <param name="isLocalMode">
    /// Whether this deployment runs in local mode, as <see cref="Innovayse.Hostpanel.API.Extensions.AuthenticationRegistration.AddHostpanelAuthentication"/>
    /// resolved it — local mode has one scheme to authenticate against, so it needs neither a
    /// non-default default policy nor an explicit scheme list on every role policy.
    /// </param>
    /// <returns>The container, for chaining.</returns>
    public static IServiceCollection AddHostpanelAuthorizationPolicies(this IServiceCollection services, bool isLocalMode)
    {
        services.AddAuthorization(opts =>
        {
            if (!isLocalMode)
            {
                // Accept tokens from either SSO or local JWT scheme
                opts.DefaultPolicy = new AuthorizationPolicyBuilder(
                    JwtBearerDefaults.AuthenticationScheme, Innovayse.Hostpanel.Shared.Constants.LocalJwtScheme.Name,
                    AuthenticationRegistration.SsoCookieScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            }

            // Every policy authenticates against the same schemes the default policy accepts; under
            // SSO that is the SSO bearer, the admin panel's local JWT and the browser session cookie.
            void AddRolePolicy(string name, params string[] roles)
            {
                opts.AddPolicy(name, p =>
                {
                    if (!isLocalMode)
                    {
                        p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, Innovayse.Hostpanel.Shared.Constants.LocalJwtScheme.Name,
                            AuthenticationRegistration.SsoCookieScheme);
                    }

                    if (roles.Length == 0)
                    {
                        p.RequireAuthenticatedUser();
                    }
                    else
                    {
                        p.RequireRole(roles);
                    }
                });
            }

            AddRolePolicy(Policies.Member);
            AddRolePolicy(Policies.Admin, Roles.Admin);
            AddRolePolicy(Policies.ResellerOrAdmin, Roles.Admin, Roles.Reseller);
            AddRolePolicy(Policies.Client, Roles.Client);
        });

        return services;
    }
}
