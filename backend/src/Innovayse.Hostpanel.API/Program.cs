#if INNOVAYSE_SSO
using Innovayse.Auth;
using Innovayse.Auth.Endpoints;
#endif
using Innovayse.Hostpanel.API;
using Innovayse.Hostpanel.API.Authorization;
using Innovayse.Hostpanel.API.Extensions;
using Innovayse.Hostpanel.Application;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Infrastructure;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;
using Innovayse.Hostpanel.API.Auth;
using Innovayse.Hostpanel.Application.Admin.Seeding;
using Innovayse.Hostpanel.Application.Auth.Seeding;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Seeding;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Localization;
using Innovayse.Hostpanel.Shared.Middleware;
using Innovayse.Hostpanel.Shared.Settings;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Never announce the web server: a Server header tells a scanner what to look up. Not a secret,
    // but the leak guard's rule is that nothing upstream is named in a response.
    builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

    // Serilog — skip in Testing environment to avoid ReloadableLogger.Freeze() conflicts
    if (!builder.Environment.IsEnvironment(EnvironmentNames.Testing))
    {
        builder.Host.UseSerilog((ctx, services, config) =>
            config.ReadFrom.Configuration(ctx.Configuration)
                  .ReadFrom.Services(services)
                  .Enrich.FromLogContext());
    }

    // The layers, each registering itself. Application first (validators), then Infrastructure
    // (EF Core, Identity, external clients, plugin discovery), then Wolverine, then the web edge --
    // localization, authentication and authorization each get their own well-named extension, since
    // a deployment mode switch or a locale rule is easier to find and to change in a file named for
    // it than as one more paragraph in this one.
    //
    // Wolverine has to register before AddApi: AddApi is what registers the Domain and Billing cron
    // hosted services, and .NET starts IHostedServices in registration order. Registered the other
    // way round, DomainScheduledJobsStartup.StartAsync reaches for the bus before Wolverine's own
    // hosted service has started the runtime, and every scheduled job throws
    // WolverineHasNotStartedException the moment the host comes up -- proven by running it that way
    // once and watching every Integration test that boots the host fail on exactly that.
    builder.Services.AddApplication();

    using var bootstrapLoggerFactory = LoggerFactory.Create(b => b.AddConsole());
    builder.Services.AddInfrastructure(builder.Configuration, bootstrapLoggerFactory);

    builder.AddHostpanelWolverine();

    builder.Services.AddApi(builder.Configuration, builder.Environment);
    builder.Services.AddHostpanelLocalization(builder.Configuration);

    var isLocalMode = builder.Services.AddHostpanelAuthentication(builder.Configuration);
    builder.Services.AddHostpanelAuthorizationPolicies(isLocalMode);

    var app = builder.Build();

    // Apply EF Core migrations on startup, in every environment except Testing —
    // the test factory runs them itself once the host is up.
    //
    // This used to be Development-only, which quietly made Development the only
    // environment a deployed host could run: name it anything else and the schema
    // simply stopped following the code, with nothing in the log to say so. Nobody
    // was going to notice until a migration was missing. The SSO has always migrated
    // this way; hostpanel is the one that did not.
    if (!app.Environment.IsEnvironment(EnvironmentNames.Testing))
    {
        using var migrScope = app.Services.CreateScope();
        var dbCtx = migrScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbCtx.Database.MigrateAsync();
    }

    // Seed roles — skip in Testing environment (migrations run after host starts via factory)
    if (!app.Environment.IsEnvironment(EnvironmentNames.Testing))
    {
        using var scope = app.Services.CreateScope();

        // Identity's own role table, and only where this deployment owns its users. A
        // deployment whose people live in the SSO does not register Identity at all, so
        // RoleManager is not there to resolve — asking for it with GetRequiredService
        // threw before the API had finished starting, and the container never came up.
        //
        // Nothing is lost by skipping it. Every grant this product makes is written to
        // subject_roles, and SubjectRoleClaimsEnricher merges that store onto the principal
        // on every JWT scheme in both modes — so a role granted there authorizes in both.
        //
        // This comment used to say authorization was decided by subject_roles in both modes
        // full stop, and that was not true: no scheme registered under AUTH_MODE=local read
        // the store at all, so local authorization was decided solely by the claims baked
        // into the token at sign-in. It is worth stating precisely, because the two role
        // tables still are not one. AspNetRoles remains scaffolding for the local
        // UserManager, and the roles it holds also reach the principal — via the token
        // LocalAuthController mints from IUserService.GetRolesAsync. A local deployment
        // therefore authorizes on the union of the two, with subject_roles the only one a
        // grant made after sign-in can land in.
        if (isLocalMode)
        {
            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in new[] { Roles.Admin, Roles.Reseller, Roles.Client })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        // Support departments — every environment, for the same reason as the roles
        // above: the ticket form cannot be submitted without at least one.
        await DefaultDepartmentsSeeder.EnsureSeededAsync(
            scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>(),
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            scope.ServiceProvider
                .GetRequiredService<IOptions<DefaultDepartmentsOptions>>().Value);

        // Storefront settings — also every environment. SettingsController can update
        // an existing key but cannot create one, so a key that was never seeded is a
        // key no operator can ever set from the admin panel.
        await PortalSettingsSeeder.EnsureSeededAsync(
            scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>(),
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>());

        // Dev seed — populate test data in Development. Local mode only: the seeder
        // creates its people through Identity's UserManager, which an SSO-owned
        // deployment does not register, and there would be nowhere local to put them
        // anyway. Running it there threw on construction and took the API down with it.
        //
        // A demo deployment gets its own, richer and credential-published seed instead, and never the
        // development one: both would create clients, and the demo's idempotency keys on its own rows.
        // Reading .Value here also validates Demo:* now, so a half-configured demo stops the start
        // with the key named rather than failing on the first request.
        //
        // A demo starts from its fixed seed: the seeders add a dataset only to an empty table, so seeding on
        // top of the previous release's rows would leave a deploy showing a mix of old and new sample data until
        // the first scheduled reset, one whole interval later. Resetting wipes and reseeds in one transaction.
        if (app.Services.GetRequiredService<IOptions<DemoOptions>>().Value.Enabled)
        {
            await scope.ServiceProvider
                .GetRequiredService<IDemoDataManager>()
                .ResetAsync(CancellationToken.None);
        }
        else if (app.Environment.IsDevelopment() && isLocalMode)
        {
            var seeder = ActivatorUtilities.CreateInstance<DevDataSeeder>(scope.ServiceProvider);
            await seeder.SeedAsync();
        }

        // The first-run setup token, and only where this deployment owns its own people.
        //
        // POST /api/auth/setup grants the Admin role, and until this existed it granted it to
        // whichever authenticated caller asked first. Registration on a standalone install is
        // public, so on a box that is reachable before its owner has finished configuring it —
        // the normal shape of a self-hosted deployment — whoever registered and claimed first
        // owned the installation. The token moves that decision to "who can read this log",
        // which the operator can and a passer-by cannot.
        //
        // Deliberately last in this block, after DevDataSeeder: a Development box already has
        // an Admin by the time this runs, so nothing is issued and no token line is printed to
        // be mistaken for one that matters.
        //
        // Nothing happens under Auth:Mode=sso. Accounts there belong to the sign-on service, so
        // the callers who can reach an authenticated endpoint at all are already the ones the
        // operator provisioned, and that path is in production use.
        if (isLocalMode)
        {
            var setupToken = await SetupTokenSeeder.EnsureIssuedAsync(
                scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>(),
                scope.ServiceProvider.GetRequiredService<ISubjectRoleStore>(),
                scope.ServiceProvider.GetRequiredService<IUnitOfWork>());

            if (setupToken is not null)
            {
                // Warning level so it survives a production log filter, and formatted to be
                // read by a person tailing `docker logs` rather than by a sink. It is repeated
                // on every boot for as long as setup is outstanding: that is what makes a
                // restart mid-setup recoverable instead of a lockout, and the token is retired
                // the moment it is used.
                //
                // The token is a template argument rather than a concatenation so that a
                // structured sink stores it as a field — this line is a secret either way, and
                // an operator who ships logs off the box should know it is in them until setup
                // completes.
                Log.Warning(
                    "FIRST-RUN SETUP IS OUTSTANDING. Nobody holds the Admin role yet. " +
                    "Open the admin panel, create your account, and paste this setup token when " +
                    "it asks for one: {SetupToken}", setupToken);
            }
        }
    }

    if (!app.Environment.IsEnvironment(EnvironmentNames.Testing))
    {
        app.UseSerilogRequestLogging();
    }

    // Above the exception handler, deliberately. This middleware sets CurrentUICulture and the
    // one below it is what turns a refusal into a response body -- run the other way round, the
    // sentence would be resolved outside the culture the request asked for and every caller would
    // read English. Supported cultures come from the Application layer's LocaleOptions so they
    // cannot drift from the .resx files they select.
    //
    // XLanguageRequestCultureProvider (Shared/Localization/) goes first, reading the platform's
    // own X-Language header; AcceptLanguageHeaderRequestCultureProvider stays right behind it as
    // the fallback. Both frontends send only Accept-Language today, so an X-Language-only switch
    // would answer every production caller in English -- the fallback is removed only once the
    // frontends send X-Language too (plan ruling C, alongside the compat shim).
    var localeOptions = app.Services.GetRequiredService<IOptions<LocaleOptions>>().Value;
    var localizationOptions = new RequestLocalizationOptions()
        .SetDefaultCulture(localeOptions.DefaultLocale)
        .AddSupportedCultures([.. LocaleOptions.SupportedLocales])
        .AddSupportedUICultures([.. LocaleOptions.SupportedLocales]);
    localizationOptions.RequestCultureProviders.Insert(
        0, new XLanguageRequestCultureProvider());
    app.UseRequestLocalization(localizationOptions);

    // Reference order (innovayse-ai's Program.cs, which Shared/Middleware/ was copied from file
    // for file): localization, then TraceIdMiddleware, then ProviderLeakGuardMiddleware, then the
    // exception handler. The trace id has to exist before anything downstream might fault, and
    // the leak guard's header sweep has to be registered before the exception handler's own
    // response write, so a fault answered here is swept the same as every other response.
    app.UseMiddleware<TraceIdMiddleware>();
    app.UseMiddleware<ProviderLeakGuardMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Forwarded headers first, in BOTH modes, and above everything that reads the request's
    // origin. Two things depend on it: Request.Scheme, which is otherwise Kestrel's view of the
    // plain-HTTP hop from nginx, and Connection.RemoteIpAddress, which is otherwise nginx itself
    // — and that address is what the rate limiter partitions anonymous callers on. Left to
    // resolve wrongly it would put every visitor in one bucket, so one person's burst refuses
    // everybody: the failure that looks most like the feature working.
    //
    // UseInnovayseAuth runs its own forwarded-header handling in the SSO branch below, with the
    // framework's default ForwardLimit of 1 — one entry short of this two-hop chain. Running ours
    // first resolves the address correctly and consumes the header, so its call finds nothing left
    // to process and cannot undo the result.
    app.UseForwardedHeaders();

    app.UseStaticFiles();
    app.UseCors();
    if (!isLocalMode)
    {
        // Forwarded headers, the CSRF check, authentication and authorisation, in the
        // order they have to be in. Bearer callers pass the CSRF check untouched — a
        // bearer header is already something a cross-site page cannot attach.
#if INNOVAYSE_SSO
        app.UseInnovayseAuth();
        // GET /api/auth/login|callback and POST /api/auth/logout, for the admin SPA.
        // Not /api/auth/me: this product's own AuthController answers it with the
        // roles the database assigns, which the token cannot carry.
        app.MapInnovayseAuth(mapMe: false);
#endif

        // The limiter has to sit after authentication — it partitions on the signed-in subject
        // and falls back to an address only for callers with no credential, so above
        // UseAuthentication it would see an empty User on every request and quietly degrade to
        // one bucket per address, which for the client portal is one bucket for every customer.
        //
        // Ideally it would sit BETWEEN authentication and authorisation, so that a request refused
        // by authorisation still spends budget. It cannot here: UseInnovayseAuth is one call that
        // does forwarded headers, the CSRF check, authentication and authorisation together, and
        // splitting it would mean re-running authentication or forking a published package. The
        // consequence is bounded and worth stating — a caller spraying an [Authorize] route with
        // no credential is refused by authorisation before the limiter counts them. That is cheap
        // to serve (no handler, no database), and every route where an unauthenticated flood is
        // actually expensive — the credential endpoints, the contact form, the registrar lookups
        // — is [AllowAnonymous], so authorisation does not short-circuit it and the budget applies.
        //
        // Not in Testing: every partition there is NoLimiter (RateLimitingExtensions), so the
        // middleware would limit nothing, but it still builds two PartitionedRateLimiters whose
        // idle-sweep timers nothing ever stops. The ASP.NET Core middleware has no disposal hook, so
        // a host that is torn down leaves both timers running, and the execution context they
        // captured while the host was being built keeps the whole disposed host reachable. A test
        // run builds a host per test class and ran out of memory on those (issue 373).
        if (!app.Environment.IsEnvironment(EnvironmentNames.Testing))
        {
            app.UseRateLimiter();
        }
    }
    else
    {
        // Above authentication, so a cross-site forgery is refused before anything reads the
        // cookie it was hoping to ride on. The SSO branch gets the equivalent check from
        // UseInnovayseAuth; local mode had none because a bearer header in an Authorization
        // header is CSRF-safe on its own, and that stopped being the whole story the moment the
        // local session moved into a cookie. It examines only requests that actually present
        // that cookie, so bearer callers and anonymous webhooks are untouched.
        app.UseMiddleware<LocalSessionCsrfMiddleware>();

        app.UseAuthentication();

        // Sliding renewal of the local session; needs the authenticated User, so it follows
        // authentication. Local mode only: the SSO branch above is unchanged.
        app.UseMiddleware<LocalSessionRenewalMiddleware>();

        // Between authentication and authorisation, which is where it belongs: User is populated,
        // so the per-subject partition works, and a request that authorisation is about to refuse
        // has already spent budget rather than being free to repeat. Not in Testing, for the reason
        // given on the SSO branch above.
        if (!app.Environment.IsEnvironment(EnvironmentNames.Testing))
        {
            app.UseRateLimiter();
        }

        app.UseAuthorization();
    }

    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Innovayse API";
        options.Theme = ScalarTheme.Purple;
        options.DefaultHttpClient = new(ScalarTarget.Shell, ScalarClient.Curl);
    });

    // Liveness for the container healthcheck. Deliberately not a database probe: this
    // answers "is the process still serving HTTP", which is the question a restart
    // policy can act on. Without it the container had no healthcheck at all and Docker
    // reported "Up" through a crash loop — main-api spent minutes restarting on an
    // unresolvable database host while `docker ps` showed nothing wrong.
    app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");

    // Rethrow, don't just log. Swallowing it made Main return normally, so the process
    // exited 0 — a container whose startup failed reported success and the orchestrator
    // had no reason to restart or alert. In the integration tests the same swallow turned
    // every startup error into "The entry point exited without ever building an IHost",
    // which names nothing about the actual cause.
    throw;
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>Entry point partial class — required for WebApplicationFactory in integration tests.</summary>
public partial class Program;
