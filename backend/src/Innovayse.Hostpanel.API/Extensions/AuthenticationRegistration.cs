#if INNOVAYSE_SSO
using Innovayse.Auth;
#endif
using Innovayse.Hostpanel.Infrastructure.Auth;
using Innovayse.Hostpanel.Infrastructure.Auth.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Innovayse.Hostpanel.API.Extensions;

/// <summary>
/// Registers every authentication scheme this deployment can run under: local JWT-only mode, or
/// SSO with a local-JWT fallback for the admin panel plus the browser session cookie.
/// </summary>
public static class AuthenticationRegistration
{
    /// <summary>
    /// The scheme that stands in front of the real ones under SSO mode. See where it is
    /// registered, below <c>AddInnovayseAuth</c>, for what it is for.
    /// </summary>
    private const string SmartAuthScheme = "InnovayseSmart";

    /// <summary>
    /// The cookie scheme <c>Innovayse.Auth</c> registers under SSO mode; the one place its name is
    /// read, so every policy and the policy scheme agree on it.
    /// </summary>
    /// <remarks>
    /// A standalone build carries no <c>Innovayse.Auth</c> (see <c>InnovayseSso</c> in
    /// <c>backend/Directory.Build.props</c>), so it can only run in local mode and this value is
    /// never read there: every use sits behind a not-local-mode check that
    /// <see cref="AddHostpanelAuthentication"/> rules out at startup.
    /// </remarks>
    internal static readonly string SsoCookieScheme =
#if INNOVAYSE_SSO
        global::Innovayse.Auth.CookieSessionHandler.SchemeName;
#else
        "InnovayseSsoUnavailable";
#endif

    /// <summary>Adds hostpanel's authentication schemes for the deployment's configured mode.</summary>
    /// <param name="services">The container.</param>
    /// <param name="configuration">Host configuration, read here for <c>Auth:Mode</c>, the JWT settings and the SSO settings.</param>
    /// <returns>Whether this deployment runs in local mode, for the callers that branch on it afterwards.</returns>
    public static bool AddHostpanelAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var isLocalMode = AuthMode.IsLocal(configuration["Auth:Mode"]);

#if !INNOVAYSE_SSO
        // A standalone build has no Innovayse.Auth, so it can only run in local mode. Saying so here
        // beats booting into an SSO configuration whose cookie scheme was never registered.
        if (!isLocalMode)
        {
            throw new InvalidOperationException(
                "This build has no Innovayse SSO support, so Auth:Mode (AUTH_MODE) must be 'local'. "
                + "Set AUTH_MODE=local, or rebuild with -p:InnovayseSso=true and access to the "
                + "Innovayse package registry.");
        }
#endif

        // The token signer itself is registered by AddInnovayseInfrastructure, in both modes — the
        // admin panel uses local tokens whatever the mode is. What stays here is the composition
        // root's own reading of the same three keys, for the validators built below: the signer and
        // the validators have to agree, so both fall back to the constants the implementation names.
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var jwtSecret = jwtOptions.SigningSecret;

        if (isLocalMode)
        {
            // Local-only mode
            if (jwtSecret.Length < 32)
            {
                throw new InvalidOperationException("Jwt:Secret must be at least 32 characters");
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts =>
                {
                    opts.RequireHttpsMetadata = false;
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(jwtSecret)),
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                        NameClaimType = "sub",
                    };

                    // Local mode had no Events at all, so nothing here ever read subject_roles —
                    // the store every grant in this product writes to. Roles came only from the
                    // claims baked into the token at sign-in, which are Identity's AspNetUserRoles,
                    // leaving the two stores disjoint: setup, admin-created clients and
                    // guest-checkout customers all wrote a grant that authorized nothing.
                    opts.Events = new JwtBearerEvents
                    {
                        // The admin SPA no longer holds this token in sessionStorage, where any
                        // script injected into the page could read it. The API writes it into an
                        // httpOnly cookie instead — the same decision the SSO path already made,
                        // for the same reason — and this is the half that reads it back.
                        //
                        // The Authorization header still wins where one is present, and has to: the
                        // client portal's Nuxt server calls this API machine-to-machine with a
                        // bearer token and has no browser to hold a cookie. The cookie is a
                        // fallback for the one caller that does.
                        //
                        // Nothing about the token itself changed. Same issuer, same audience, same
                        // fifteen-minute lifetime, same validation parameters above.
                        OnMessageReceived = context =>
                        {
                            if (string.IsNullOrEmpty(context.Token)
                                && context.Request.Cookies.TryGetValue(
                                    Innovayse.Hostpanel.API.Auth.LocalSessionCookie.Name, out var cookieToken)
                                && !string.IsNullOrEmpty(cookieToken))
                            {
                                context.Token = cookieToken;
                            }

                            return Task.CompletedTask;
                        },
                        OnTokenValidated = Innovayse.Hostpanel.API.Auth.SubjectRoleClaimsEnricher.OnTokenValidated(),
                    };
                });
        }
        else
        {
            // SSO mode with local JWT fallback — admin panel uses local tokens,
            // client panel uses SSO tokens. Both are accepted.
            var localJwtIssuer = jwtOptions.Issuer;
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts =>
                {
                    opts.Authority = configuration["Sso:Authority"];
                    opts.Audience = configuration["Sso:ClientId"];
                    opts.RequireHttpsMetadata = false;
                    opts.MapInboundClaims = false;
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                        NameClaimType = "sub",
                        ValidateAudience = false,
                    };
                    // If the JWT was issued by our local server, forward to LocalJwt scheme
                    opts.ForwardDefaultSelector = context =>
                    {
                        var auth = context.Request.Headers.Authorization.FirstOrDefault();
                        if (auth?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            var token = auth["Bearer ".Length..];
                            try
                            {
                                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                                var jwt = handler.ReadJwtToken(token);
                                if (jwt.Issuer == localJwtIssuer)
                                {
                                    return "LocalJwt";
                                }
                            }
                            catch { /* not a valid JWT — let default scheme handle */ }
                        }
                        return null; // use default (SSO) scheme
                    };
                    // The subject the SSO issued is the identifier this product uses, so there is
                    // nothing to map it onto. This used to provision a local copy of the user and
                    // swap NameIdentifier to that copy's id; the copy was written once and never
                    // updated, so a name or address changed in the SSO never reached here.
                    //
                    // The role merge itself is shared with both locally issued schemes rather than
                    // written inline here — it was the only reader of subject_roles in the process,
                    // which is what made that store SSO-only in practice.
                    opts.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = Innovayse.Hostpanel.API.Auth.SubjectRoleClaimsEnricher.OnTokenValidated(),
                    };
                })
                .AddJwtBearer("LocalJwt", opts =>
                {
                    opts.RequireHttpsMetadata = false;
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(jwtSecret)),
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                        NameClaimType = "sub",
                    };

                    // Same merge as the SSO scheme above. This one had no Events either, and the
                    // ForwardDefaultSelector sends every token this product minted itself here —
                    // so the admin panel's own credential was the one credential in SSO mode that
                    // could not see a role granted through subject_roles.
                    opts.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = Innovayse.Hostpanel.API.Auth.SubjectRoleClaimsEnricher.OnTokenValidated(),
                    };
                });
        }

        if (!isLocalMode)
        {
            // Cookie sessions, the third way in — and the only one a browser uses now.
            // The admin SPA held an access AND refresh token in localStorage, where any
            // script on the page could read a 30-day credential; with this the API runs
            // the OIDC exchange itself and the browser holds an opaque httpOnly cookie.
            //
            // Bearer stays for the machines: the client portal's Nuxt server calls this
            // API with an SSO token, and standalone deployments keep LocalJwt. That is
            // why this is an additional scheme rather than a replacement.
#if INNOVAYSE_SSO
            services.AddInnovayseAuth(configuration);
#endif

            // …except that registering it made it the default scheme, which is not what
            // "additional" means. Everything below turns on the difference between the
            // default POLICY and the default SCHEME:
            //
            //   [Authorize]                     uses DefaultPolicy, which names all three
            //                                   schemes below and therefore worked.
            //   [Authorize(Roles = "Client")]   does not. Naming a role makes MVC build a
            //                                   policy on the spot, and that policy carries
            //                                   no schemes, so it falls back to the default
            //                                   authenticate scheme — the cookie.
            //
            // So every one of the 85 role-annotated endpoints stopped reading the
            // Authorization header. A caller presenting a perfectly good SSO token was
            // challenged for a cookie it does not have and got a bare 401, with no
            // WWW-Authenticate to say why. That is the whole client portal: its Nuxt server
            // calls this API with a bearer token, so /api/me/* answered 401 to every
            // request and the customer's dashboard rendered empty.
            //
            // A policy scheme restores the intent. It authenticates nothing itself; it
            // picks the real scheme per request, so a bearer token is read as a bearer
            // token whether or not the attribute happened to name a role.
            services.AddAuthentication(opts =>
                {
                    opts.DefaultAuthenticateScheme = SmartAuthScheme;
                    opts.DefaultChallengeScheme = SmartAuthScheme;
                })
                .AddPolicyScheme(SmartAuthScheme, "Bearer when one is offered, cookie otherwise", opts =>
                {
                    opts.ForwardDefaultSelector = context =>
                        context.Request.Headers.Authorization.FirstOrDefault()
                            ?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
                                // The bearer handler has its own selector that sends locally
                                // issued tokens on to LocalJwt, so this only has to choose
                                // between "a token" and "a cookie".
                                ? JwtBearerDefaults.AuthenticationScheme
                                : SsoCookieScheme;
                });

            // The cookie principal carries the SSO's claims; the roles live in this
            // database. The transformation maps one onto the other.
#if INNOVAYSE_SSO
            services.AddScoped<Microsoft.AspNetCore.Authentication.IClaimsTransformation,
                SsoSessionClaimsTransformation>();
#endif
        }

        return isLocalMode;
    }
}
