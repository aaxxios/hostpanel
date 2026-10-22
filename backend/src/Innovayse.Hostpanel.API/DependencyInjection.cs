using System.Text.Json.Serialization;
using Innovayse.Hostpanel.API.Identity;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Infrastructure.Scheduling;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Filters;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Innovayse.Hostpanel.Shared.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Innovayse.Hostpanel.API;

/// <summary>
/// Registers what the web edge owns: CORS and the redirect-safe origins list, the client
/// portal and compatibility options, the request-scoped identity adapter, MVC and OpenAPI, the
/// action-timing filter, the ProblemDetails surfaces that answer before a controller runs, the
/// domain and billing cron jobs, and the edge rate limiter.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Adds the API layer.</summary>
    /// <param name="services">The container.</param>
    /// <param name="configuration">Host configuration, read here and nowhere below.</param>
    /// <param name="environment">The host environment, needed by the rate limiter's Testing-environment bypass.</param>
    /// <returns>The container, for chaining.</returns>
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // CORS
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(opts =>
            opts.AddDefaultPolicy(policy =>
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials()));

        // Not a section of its own. A payer may only be handed back to an origin the web edge
        // already trusts, so the list is the CORS one read just above rather than a second copy
        // that could drift from it -- see GatewayReturnUrlOptions for why it carries no SectionName.
        services.AddOptions<GatewayReturnUrlOptions>()
            .Configure(o => o.AllowedOrigins = allowedOrigins)
            .ValidateOnStart();

        // ClientBaseUrl and DefaultLocale are bare top-level keys, never sections, so they are read
        // by key here and left at the option class's default when the deployment sets neither.
        var clientBaseUrl = configuration[ClientPortalOptions.ConfigurationKey];
        services.AddOptions<ClientPortalOptions>()
            .Configure(o =>
            {
                if (!string.IsNullOrWhiteSpace(clientBaseUrl))
                {
                    o.BaseUrl = clientBaseUrl;
                }
            })
            .Validate(
                o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _),
                $"{ClientPortalOptions.ConfigurationKey} must be an absolute URL including the scheme, "
                    + "e.g. https://client.example.com.")
            .ValidateOnStart();

        // AdminBaseUrl is the same kind of key: where staff land from a mailed link.
        var adminBaseUrl = configuration[AdminPanelOptions.ConfigurationKey];
        services.AddOptions<AdminPanelOptions>()
            .Configure(o =>
            {
                if (!string.IsNullOrWhiteSpace(adminBaseUrl))
                {
                    o.BaseUrl = adminBaseUrl;
                }
            })
            .Validate(
                o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _),
                $"{AdminPanelOptions.ConfigurationKey} must be an absolute URL including the scheme, "
                    + "e.g. https://admin.example.com.")
            .ValidateOnStart();

        // Contact addresses for the support departments seeded on a fresh install.
        services.AddOptions<DefaultDepartmentsOptions>()
            .Bind(configuration.GetSection(DefaultDepartmentsOptions.SectionName))
            .Validate(
                o => o.IsValid,
                $"{DefaultDepartmentsOptions.SectionName} addresses must each be a mailbox such as support@example.com.")
            .ValidateOnStart();

        // The caller of the request, for handlers. One scoped instance answers both ports, so the
        // display name it looks up is resolved once per request whichever port asked first.
        services.AddHttpContextAccessor();
        services.AddScoped<HttpCurrentUserService>();
        services.AddScoped<ICurrentUserService>(sp => sp.GetRequiredService<HttpCurrentUserService>());

        // MVC + OpenAPI
        services.AddMemoryCache();
        services.AddControllers(opts =>
            {
                // Every 201 Created in this API is built with CreatedAtAction(nameof(GetAsync), …),
                // and MVC trims the "Async" suffix from action names by default — so the name never
                // matched and the framework threw "No route matches the supplied values". The request
                // had already succeeded by then: the row was written, then the response failed, and
                // the caller saw a 400 for a creation that worked. An admin clicking again got a
                // duplicate.
                //
                // Five controllers do this — TldConfigs, Slides, ServerGroups, Clients and
                // EmailTemplates — so the fix belongs here rather than in each of them. No action
                // name is referenced as a string anywhere, so nothing depends on the trimmed form.
                opts.SuppressAsyncSuffixInActionNames = false;

                // Required-ness is the validators' business, where every message is localized. Without this MVC
                // makes a non-nullable string an implicit [Required] with a fixed English message, so a body
                // that leaves out a route-supplied id (`command with { MemberUserId = userId }`) is a 400
                // before the handler can overlay it.
                opts.ConfigureLocalizedModelBinding();
            })
            .AddJsonOptions(opts =>
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // Three of the error surfaces that short-circuit before a controller or the exception
        // middleware runs (rules/api.md): model binding, the bearer challenge/forbid, and an
        // authorization-policy refusal. Each writes the one ErrorResponse body instead of the
        // framework's ValidationProblemDetails or an empty 401/403.
        // BaseApiController carries [ServiceFilter(typeof(ActionTimingFilter))], so the filter and its
        // options are resolved per action and must be registered here; without them every action on a
        // BaseApiController controller fails before it runs.
        services.AddOptions<ActionTimingOptions>()
            .BindConfiguration(ActionTimingOptions.SectionName)
            .ValidateOnStart();
        services.AddScoped<ActionTimingFilter>();

        services.ConfigureModelStateProblemDetails();
        services.ConfigureProblemDetailsAuthEvents();
        services.PostConfigure<JwtBearerOptions>(LocalJwtScheme.Name, o => o.UseProblemDetailsResponses());
        services.AddProblemDetailsAuthorizationResponses();
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((doc, ctx, ct) =>
            {
                doc.Info = new()
                {
                    Title = "Innovayse API",
                    Version = "v1",
                    Description = "Innovayse hosting panel API — self-hosted hosting management platform built with ASP.NET Core",
                    Contact = new() { Name = "Hostpanel" },
                };
                doc.Components ??= new();
                doc.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
                doc.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter JWT token (without 'Bearer' prefix)",
                };
                return Task.CompletedTask;
            });
            options.AddOperationTransformer((operation, context, ct) =>
            {
                operation.Security ??= [];
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme,
                        },
                    }] = [],
                });
                return Task.CompletedTask;
            });
        });

        // The two cron startups below each hand Wolverine a message to deliver hours from now. Wolverine
        // keeps that delay as a timer in the process-wide timer queue and does not cancel it when the
        // host stops, so in a process that starts and stops many hosts (the integration tests) every
        // stopped host stays reachable from the timer, with its whole service graph and the code
        // Wolverine compiled for it. The tests drive the crons by dispatching their messages directly
        // and never wait for the clock, so the Testing environment does not schedule them (issue 373).
        if (!environment.IsEnvironment(EnvironmentNames.Testing))
        {
            // Domain scheduled jobs — daily expiry check (09:00 UTC) and auto-renew (10:00 UTC)
            services.AddHostedService<DomainScheduledJobsStartup>();

            // Billing scheduled jobs — daily billable items cron processing (06:00 UTC)
            services.AddHostedService<BillingScheduledJobsStartup>();
        }

        // Demo data reset — schedules the first run only when Demo:Enabled is true; a no-op otherwise.
        // Registered in every environment because the demo tests read the reset time it publishes.
        services.AddHostedService<DemoScheduledJobsStartup>();

        // Rate limiting, and the forwarded-header handling it depends on. Registered as one call
        // because both are the web edge's reading of the same proxy chain, and a limiter whose idea
        // of "the caller" disagrees with the rest of the pipeline's is worse than none: it would
        // partition every browser onto nginx's address and let one visitor's burst refuse everybody.
        //
        // Everything is limited by a global budget whether or not its action names a policy — see
        // RateLimitingExtensions for why an opt-in scheme does not survive fifty-eight controllers.
        services.AddPlatformRateLimiting(configuration, environment);

        return services;
    }
}
