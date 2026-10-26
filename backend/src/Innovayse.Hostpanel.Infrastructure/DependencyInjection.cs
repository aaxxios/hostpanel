using Innovayse.Hostpanel.Application.Billing.CurrencyResolution;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Application.Notifications.Rendering;
using Innovayse.Hostpanel.Application.Notifications.Seeding;
using Innovayse.Hostpanel.Infrastructure.Auth;
using Innovayse.Hostpanel.Infrastructure.Auth.Settings;
using Innovayse.Hostpanel.Infrastructure.Currency;
using Innovayse.Hostpanel.Infrastructure.Demo;
using Innovayse.Hostpanel.Infrastructure.Email;
using Innovayse.Hostpanel.Infrastructure.Email.Settings;
using Innovayse.Hostpanel.Infrastructure.Integrations.CPanel;
using Innovayse.Hostpanel.Infrastructure.Integrations.CPanel.Settings;
using Innovayse.Hostpanel.Infrastructure.Integrations.Migration;
using Innovayse.Hostpanel.Infrastructure.Integrations.NameAm;
using Innovayse.Hostpanel.Infrastructure.Integrations.NameAm.Settings;
using Innovayse.Hostpanel.Infrastructure.Integrations.Namecheap;
using Innovayse.Hostpanel.Infrastructure.Integrations.Namecheap.Settings;
using Innovayse.Hostpanel.Infrastructure.Integrations.Stripe;
using Innovayse.Hostpanel.Infrastructure.Integrations.Stripe.Settings;
using Innovayse.Hostpanel.Infrastructure.Integrations.Telegram;
using Innovayse.Hostpanel.Infrastructure.Integrations.Telegram.Settings;
using Innovayse.Hostpanel.Infrastructure.Payments;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Infrastructure.Plugins;
using Innovayse.Hostpanel.Infrastructure.Provisioning;
using Innovayse.Hostpanel.Infrastructure.Registrar;
using Innovayse.Hostpanel.Infrastructure.Resilience;
using Innovayse.Hostpanel.Infrastructure.Resilience.Extensions;
using Innovayse.Hostpanel.Infrastructure.Resilience.Settings;
using Innovayse.Hostpanel.Infrastructure.Security;
using Innovayse.Hostpanel.Infrastructure.Storage;
using Innovayse.Providers.CWP;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using IProvisioningProvider = Innovayse.Hostpanel.Application.Common.Interfaces.IProvisioningProvider;

namespace Innovayse.Hostpanel.Infrastructure;
/// <summary>
/// Registers all Infrastructure layer services into the DI container.
/// Call <see cref="AddInfrastructure"/> from <c>Program.cs</c>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds EF Core, Identity, JWT token services, Infrastructure repositories, and discovers plugins.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="loggerFactory">Logger factory used during plugin discovery at startup.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        ILoggerFactory loggerFactory)
    {
        // Encryption.
        //
        // An absent key used to mean "skip encryption", and appsettings.json ships
        // EncryptionKey as "". Nothing failed: EncryptionServiceHolder.CreateConverter
        // returned null, the EF configurations took their `else` branch, and server
        // passwords, API tokens, access hashes and client-service passwords were
        // written to the database in plain text — on a deployment that looked healthy
        // and logged nothing.
        //
        // So outside Development it is required. Development still allows it to be
        // absent, because a developer's database holds nothing worth protecting and
        // requiring a key would only be worked around.
        var encryptionKey = configuration["EncryptionKey"];
        var isDevelopment = string.Equals(
            configuration["ASPNETCORE_ENVIRONMENT"], "Development", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(encryptionKey))
        {
            if (!isDevelopment)
            {
                throw new InvalidOperationException(
                    "EncryptionKey is not set. Server credentials and client-service "
                    + "passwords would be stored unencrypted. Generate one with: "
                    + "openssl rand -base64 32");
            }
        }
        else
        {
            var encryptionService = new AesEncryptionService(encryptionKey);
            services.AddSingleton<IEncryptionService>(encryptionService);
            EncryptionServiceHolder.Instance = encryptionService;
        }

        // Outbound HTTP resilience.
        //
        // Registered first because every AddHttpClient below reaches for it. Until this existed
        // not one of the eleven client registrations in this file had a retry, a breaker or a
        // timeout of its own, so a registrar or a control panel that stopped answering held a
        // request thread for HttpClient's 100-second default -- and a third party having a bad
        // ten minutes became this platform having a bad ten minutes.
        //
        // The section is optional: every profile carries the measured default in
        // HttpResilienceOptions, and an operator only names one to move it. It is validated on
        // start rather than at first use, because the first use of the cPanel profile is a
        // customer's provisioning run.
        services.AddOptions<HttpResilienceOptions>()
            .Bind(configuration.GetSection(HttpResilienceOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<
            IValidateOptions<HttpResilienceOptions>, HttpResilienceOptionsValidator>();

        // EF Core
        // Domain events are delivered by an interceptor on the context, per scope: the gate lets a
        // bulk writer (migration pull, dev seed) discard events, and the interceptor reads it.
        services.AddScoped<IDomainEventGate, DomainEventGate>();
        services.AddScoped<DomainEventDispatchInterceptor>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetRequiredService<DomainEventDispatchInterceptor>());

            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);

                    // One SQL statement per collection navigation rather than one join
                    // over all of them. DomainRepository includes four collections on
                    // every read, and the single-query default multiplies their rows
                    // together — EF warned about it on each domain cron run in
                    // production. A query that needs the single statement's snapshot
                    // consistency opts back in with AsSingleQuery(); none does today.
                    npgsql.UseQuerySplittingBehavior(
                        Microsoft.EntityFrameworkCore.QuerySplittingBehavior.SplitQuery);
                });

            // Suppress PendingModelChangesWarning so MigrateAsync() works in Development
            // even when the snapshot is slightly out of sync with the model.
            options.ConfigureWarnings(w =>
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        // The repository handlers write against: the same scoped context instance, seen through the
        // Application-layer port so no handler references Infrastructure.
        services.AddScoped<IHostpanelDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IClientRecordStore, ClientRecordStore>();
        services.AddScoped<INumberSequence, NumberSequence>();

        // Storefront branding uploads. Both ports are stateless, so they are registered as
        // singletons: the processor holds only the ceilings it was configured with, and the
        // storage resolves the web root per call.
        services.AddOptions<Innovayse.Hostpanel.Infrastructure.Storage.Settings.BrandingOptions>()
            .Bind(configuration.GetSection(
                Innovayse.Hostpanel.Infrastructure.Storage.Settings.BrandingOptions.SectionName))
            .Validate(
                o => o.MaxBytes > 0 && o.MaxSourceEdge > 0 && o.MaxLogoEdge > 0,
                "Branding ceilings must all be positive.")
            .Validate(
                o => !Path.IsPathRooted(o.RelativePath) && !o.RelativePath.Contains(".."),
                "Branding:RelativePath must stay inside the web root.")
            .ValidateOnStart();
        services.AddSingleton<Innovayse.Hostpanel.Application.Common.Interfaces.IBrandingImageProcessor,
            Innovayse.Hostpanel.Infrastructure.ImageProcessing.BrandingImageProcessor>();
        services.AddSingleton<Innovayse.Hostpanel.Application.Common.Interfaces.IBrandingStorage,
            Innovayse.Hostpanel.Infrastructure.Storage.FileSystemBrandingStorage>();

        // Where people live decides almost everything below. Read from configuration once,
        // here, so there is one answer rather than a scattering of Auth:Mode checks that
        // can disagree.
        var ownsItsUsers = AuthMode.IsLocal(configuration["Auth:Mode"]);

        // The one DI-resolvable answer to the same question, for everything downstream
        // of service registration that used to run its own inline comparison.
        services.AddSingleton<IAuthModeProvider>(new ConfigurationAuthModeProvider(ownsItsUsers));

        // The sign-out fan-out's key: required under sso, optional in local mode (an unset key
        // refuses every call). The validator asks the mode provider registered just above.
        services.AddSingleton<IValidateOptions<BackchannelLogoutOptions>, BackchannelLogoutOptionsValidator>();
        services.AddOptions<BackchannelLogoutOptions>()
            .Bind(configuration.GetSection(BackchannelLogoutOptions.SectionName))
            .ValidateOnStart();

        // The signer's settings; the composition root reads the same class for its validators.
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateOnStart();

        // Session lifetimes: access-token length and the absolute maximum age of a renewed session.
        services.AddSingleton<IValidateOptions<SessionOptions>, SessionOptionsValidator>();
        services.AddOptions<SessionOptions>()
            .Bind(configuration.GetSection(SessionOptions.SectionName))
            .ValidateOnStart();
        Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddSingleton(services, TimeProvider.System);

        // AppName is a bare top-level key, not a section; unset keeps the class default.
        var appName = configuration[LocalTwoFactorOptions.ConfigurationKey];
        services.AddOptions<LocalTwoFactorOptions>()
            .Configure(o =>
            {
                if (!string.IsNullOrWhiteSpace(appName))
                {
                    o.Issuer = appName;
                }
            })
            .ValidateOnStart();

        // The local token signer, registered in both modes on purpose: the admin SPA signs in
        // with a local JWT whatever owns the accounts, so the port must resolve even where an
        // SSO issues the client portal's tokens. Singleton — it holds no per-request state.
        services.AddSingleton<Innovayse.Hostpanel.Application.Common.Interfaces.IJwtService, JwtTokenService>();

        if (ownsItsUsers)
        {
            // ASP.NET Core Identity — use AddIdentityCore to avoid overriding the JWT Bearer
            // authentication scheme that is configured in Program.cs.
            // AddIdentity would reset the default auth scheme to cookie-based Identity,
            // causing API endpoints to redirect to /Account/Login instead of returning 401.
            services.AddIdentityCore<AppUser>(opts =>
                {
                    opts.Password.RequiredLength = 8;
                    opts.Password.RequireDigit = false;
                    opts.Password.RequireNonAlphanumeric = false;
                    opts.User.RequireUniqueEmail = true;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
        }

        // Auth services
        // Roles live in this product's own database in both modes, keyed by whatever the
        // configured identity provider calls a person. Registered outside the Identity
        // block above because it does not depend on Identity being registered at all —
        // which is the point, since SSO mode will stop registering it.
        services.AddScoped<Innovayse.Hostpanel.Application.Common.Interfaces.ISubjectRoleStore, SubjectRoleStore>();

        // One provider, never both. In SSO mode ASP.NET Identity is not registered at all,
        // so there is no code path that could write a person row even by accident — which
        // is what let a second, drifting copy of every user exist before.
        if (ownsItsUsers)
        {
            services.AddScoped<Innovayse.Hostpanel.Application.Common.Interfaces.IIdentityProvider, LocalIdentityProvider>();
            services.AddScoped<Innovayse.Hostpanel.Application.Common.Interfaces.IUserProvisioning, LocalUserProvisioning>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<Innovayse.Hostpanel.Application.Common.Interfaces.ITwoFactorService, LocalTwoFactorService>();
            services.AddScoped<Innovayse.Hostpanel.Application.Common.Interfaces.ISessionRevoker, LocalSessionRevoker>();
        }
        else
        {
            services.AddScoped<Innovayse.Hostpanel.Application.Common.Interfaces.IIdentityProvider, SsoIdentityProvider>();
            services.AddScoped<Innovayse.Hostpanel.Application.Common.Interfaces.IUserProvisioning, SsoModeUserProvisioning>();
            services.AddScoped<Innovayse.Hostpanel.Application.Common.Interfaces.ITwoFactorService, SsoTwoFactorService>();
#if INNOVAYSE_SSO
            // Sessions live in the shared Innovayse.Auth store, so revoking is that store's call.
            services.AddScoped<Innovayse.Hostpanel.Application.Common.Interfaces.ISessionRevoker, SsoSessionRevoker>();
#else
            // A standalone build cannot run in sso mode (the API refuses to boot), so this is
            // never resolved; it exists so the container is complete in every build.
            services.AddScoped<Innovayse.Hostpanel.Application.Common.Interfaces.ISessionRevoker, LocalSessionRevoker>();
#endif

            // The SSO's service API, addressed by the same authority the token validation
            // uses. The service key is this product's own credential, not a user's.
            services.AddHttpClient<SsoServiceClient>(client =>
            {
                var authority = configuration["Sso:Authority"]
                    ?? throw new InvalidOperationException(
                        "Sso:Authority must be set when Auth:Mode is not 'local' — "
                        + "it is where this product reads its people from.");
                client.BaseAddress = new Uri(authority.TrimEnd('/') + "/");
                client.DefaultRequestHeaders.Add("X-Service-Key", configuration["Sso:ServiceKey"] ?? string.Empty);
            })
            // Every operation on this client is a GET that reads a person, so all of them are
            // safe to repeat. The breaker's shape -- and the fact that an open one means nobody
            // is looked up -- is argued on HttpResilienceOptions.SsoRead.
            .AddReadOnlyResilience(o => o.SsoRead);

            // The SSO's TOTP endpoints, addressed the same way — but unlike SsoServiceClient
            // above, no X-Service-Key: two-factor acts as the calling person, not as the
            // platform, so each call carries the caller's own bearer token instead (see
            // SsoTwoFactorService).
            services.AddHttpClient<SsoTwoFactorClient>(client =>
            {
                var authority = configuration["Sso:Authority"]
                    ?? throw new InvalidOperationException(
                        "Sso:Authority must be set when Auth:Mode is not 'local' — "
                        + "it is where this product reads its people from.");
                client.BaseAddress = new Uri(authority.TrimEnd('/') + "/");
            })
            // No retry stage. All three TOTP operations are POSTs that change a person's second
            // factor: enable issues a fresh secret, so a repeat hands back a different QR code
            // from the one already on screen, and verify and disable each spend a one-time code.
            .AddNoRetryResilience(o => o.SsoTwoFactor);
        }

        // Client services. Client-user links and invitations are read and written straight
        // through IHostpanelDbContext by the Clients handlers; IClientRepository is gone too --
        // Orders, Billing, Domains, Services and Support now read and write clients the same way
        // (A6 Task 3).

        // Domain ownership rule for the client-facing routes. Fully qualified for the same
        // reason as the export repository above: the Application layer's Domains namespace
        // would shadow the Domain one.
        //
        // MyDomainsController no longer injects this. The eighteen client-facing My* handlers
        // call it themselves, so the check travels with the message -- which matters here
        // because every command behind those endpoints is also dispatched by the admin
        // DomainsController, and RenewDomainCommand by the auto-renew job as well.
        services.AddScoped<
            Innovayse.Hostpanel.Application.Common.Interfaces.IDomainOwnership,
            Innovayse.Hostpanel.Application.Domains.Authorization.DomainOwnership>();

        // The same rule for support tickets and for invoices, in the same shape.
        services.AddScoped<
            Innovayse.Hostpanel.Application.Common.Interfaces.ITicketOwnership,
            Innovayse.Hostpanel.Application.Support.Authorization.TicketOwnership>();
        services.AddScoped<
            Innovayse.Hostpanel.Application.Common.Interfaces.IInvoiceOwnership,
            Innovayse.Hostpanel.Application.Billing.Authorization.InvoiceOwnership>();

        // The same rule for client services. New with GetMyServiceInvoicesQuery, which is the
        // first client-facing service use case to scope itself to the caller at all; the five
        // older routes on MyServicesController still take a service id off the route unchecked.
        services.AddScoped<
            Innovayse.Hostpanel.Application.Common.Interfaces.IServiceOwnership,
            Innovayse.Hostpanel.Application.Services.Authorization.ServiceOwnership>();

        // What the signed-in user may do on a client account. The portal's permission filter decides
        // with it and the account endpoint reports it; both read it per request and neither trusts
        // anything the request says about it.
        services.AddScoped<
            Innovayse.Hostpanel.Application.Common.Interfaces.IClientAccess,
            Innovayse.Hostpanel.Application.Clients.Authorization.ClientAccess>();

        // Product services — IProductFeatureRepository, IProductGroupRepository and
        // IProductRepository are all gone: every consumer across Products, Migration, Domains,
        // Services and Billing now reads and writes through IHostpanelDbContext (A6 Task 5).

        // Slides — ISlideRepository is gone: Slides/ was its only consumer and every handler
        // there now writes through IHostpanelDbContext. The image upload action is not a
        // Result-returning use case for the database, so it keeps its own small port.
        services.AddScoped<ISlideImageStorage, LocalSlideImageStorage>();

        // Service provisioning. IClientServiceRepository and ICancellationRequestRepository are
        // gone: every remaining consumer (crons, event handlers, ownership resolvers, the Services
        // handlers) now reads and writes through IHostpanelDbContext (A6 Task 3).
        // Bound and checked at startup: either the whole section is unset -- a deployment that
        // provisions through per-server credentials held in the database instead -- or it is
        // complete. A half-filled section used to bind silently and fail on the first call.
        services.AddOptions<CPanelOptions>()
            .Bind(configuration.GetSection(CPanelOptions.SectionName))
            .Validate(
                o => o.IsUsable,
                $"{CPanelOptions.SectionName} is partly configured: ApiUrl, Username and ApiToken "
                    + "must either all be set or the whole section left unset.")
            .ValidateOnStart();
        services.AddHttpClient<CPanelClient>((sp, httpClient) =>
        {
            var settings = sp.GetRequiredService<IOptions<CPanelOptions>>().Value;

            // Resolving this client at all means something is about to call WHM, so an unset
            // section is an error here even though it is allowed at startup -- said plainly and
            // naming the setting, rather than as the UriFormatException an empty URL produces.
            if (!settings.IsConfigured)
            {
                throw new InvalidOperationException(
                    $"cPanel provisioning was requested but the \"{CPanelOptions.SectionName}\" "
                    + "configuration section is not set.");
            }

            httpClient.BaseAddress = new Uri(settings.ApiUrl);

            // Kept as the outer backstop only. The resilience pipeline below decides in 45s and
            // 55s; this catches a pipeline that was configured wrong, nothing else.
            httpClient.Timeout = HttpBackstopTimeouts.CPanel;
            httpClient.DefaultRequestHeaders.Add(
                HeaderNames.Authorization,
                $"WHM {settings.Username}:{settings.ApiToken}");
        })
            // No retry stage, and the HTTP method is no guide: WHM's JSON API v1 is addressed
            // entirely over GET, so every one of the seven functions this client calls --
            // createacct, removeacct, passwd among them -- is a write dressed as a GET. A
            // method-based predicate would repeat all of them.
            .AddNoRetryResilience(o => o.CPanel);
        // Use NullCPanelProvisioningProvider as fallback for unmigrated code
        services.AddScoped<IProvisioningProvider, NullCPanelProvisioningProvider>();

        // Provisioning behaviour switches. Left unset everywhere but development, where
        // appsettings.Development.json turns UseFakeProvider on so the setup flow can complete
        // without a live CWP7 panel to call.
        services.AddOptions<Innovayse.Hostpanel.Infrastructure.Provisioning.Settings.ProvisioningOptions>()
            .Bind(configuration.GetSection(
                Innovayse.Hostpanel.Infrastructure.Provisioning.Settings.ProvisioningOptions.SectionName))
            .ValidateOnStart();

        // Provisioning provider factory — creates per-server providers (CWP7, cPanel, etc.)
        services.AddScoped<Innovayse.Hostpanel.Application.Common.Interfaces.IProvisioningProviderFactory, Innovayse.Hostpanel.Infrastructure.Provisioning.ProvisioningProviderFactory>();

        // Billing services
        services.AddScoped<IPaymentGateway, NullPaymentGateway>();
        services.AddScoped<IPayerCurrencyResolver, PayerCurrencyResolver>();

        // Exchange rates come from the Central Bank of Armenia's bulletin. The URL and its shape
        // are argued on CbaExchangeRateSource; the read-only profile with retries is argued on
        // HttpResilienceOptions.Cba. The interface is what the handler depends on, so another
        // bank is another class here and nothing in Application moves.
        services.AddHttpClient<IExchangeRateSource, CbaExchangeRateSource>(client =>
        {
            client.BaseAddress = new Uri(CbaExchangeRateSource.BaseAddress);
        })
            .AddReadOnlyResilience(o => o.Cba);

        // Stripe
        // Optional in the same way cPanel is: no Stripe section at all is a deployment that takes
        // no card payments, and StripeService fails on the first call that needs a key. A section
        // with only some of the keys filled in is refused here instead.
        services.AddOptions<StripeOptions>()
            .Bind(configuration.GetSection(StripeOptions.SectionName))
            .Validate(
                o => o.IsUsable,
                $"{StripeOptions.SectionName} is partly configured: SecretKey must be set, or the "
                    + "whole section left unset.")
            .ValidateOnStart();
        services.AddScoped<ICardPaymentGateway, StripeService>();

        // Payment plugins (hosted-gateway providers, e.g. Inecobank)
        services.AddHttpClient();

        // The factory's unnamed client -- what CreateClient() with no argument hands back. It is
        // used by CpanelWhmApi for per-server WHM calls and by any plugin that asks for a client
        // without naming one, so nobody knows what it calls: a plugin's POST may well be a
        // charge. Unknown means unrepeatable, and no breaker, since every caller addresses a
        // different host. AddHttpClient() above only registers the factory; the empty name is
        // what configures the default client itself.
        services.AddHttpClient(string.Empty)
            .AddNoRetryResilience(o => o.Default);

        // The Inecobank gateway plugin's own named client.
        //
        // The plugin asks the factory for its plugin id, and until this existed that name matched
        // no registration -- and IHttpClientFactory answers an unknown name with a default client
        // rather than an error. So the one payment gateway in this product was also the last
        // 100-second path in it: no handler, no retry, no breaker, and a bank having a bad
        // afternoon holding a checkout thread for a minute and a half.
        //
        // Registered here rather than in the provider because the provider has no composition
        // root of its own: PluginLoader reflects it out of plugins/ and Infrastructure
        // deliberately does not reference it, so the client name is the whole contract between
        // the two ends. It is spelled once, on HttpClientResilienceExtensions.
        //
        // Retries only getOrderStatusExtended.do. register.do and refund.do are POSTs to the same
        // host over the same verb, so the method separates nothing and the predicate reads the
        // endpoint out of the path -- a repeated refund.do is a second refund. The timeout, and
        // why this money-moving client gets a breaker when the others do not, are argued on
        // HttpResilienceOptions.Inecobank.
        services.AddHttpClient(HttpClientResilienceExtensions.InecobankClientName, client =>
        {
            // Outer backstop only. The pipeline below decides in 15s and 40s; this catches a
            // pipeline that was configured wrong, nothing else.
            client.Timeout = HttpBackstopTimeouts.Inecobank;
        })
            .AddInecobankResilience();

        services.AddScoped<IPaymentPluginResolver, PaymentPluginResolver>();

        // Audit
        services.AddHttpContextAccessor();

        // Telegram — the operator's chat channel for public contact-form enquiries.
        // Optional in the same way Stripe and Name.am are: an unset section is a deployment that
        // runs no bot, and the notifier logs that it did nothing rather than failing an enquiry
        // the mail already delivered. A half-filled section is refused here instead, because a
        // token with no chat id posts nowhere while looking configured.
        //
        // The token is a path segment of every Bot API call rather than a header, so it cannot be
        // baked into the base address at registration; the notifier reads it from options per
        // call. The base address is the API root and ends in a slash — without it, the relative
        // "bot<token>/sendMessage" would replace the last segment instead of extending the path.
        services.AddOptions<TelegramOptions>()
            .Bind(configuration.GetSection(TelegramOptions.SectionName))
            .Validate(
                o => Uri.TryCreate(o.ApiUrl, UriKind.Absolute, out _) && o.ApiUrl.EndsWith('/'),
                $"{TelegramOptions.SectionName}:{nameof(TelegramOptions.ApiUrl)} must be an absolute URL ending in a slash.")
            .Validate(
                o => o.IsUsable,
                $"{TelegramOptions.SectionName} is partly configured: BotToken and ChatId must "
                    + "either both be set or the whole section left unset.")
            .ValidateOnStart();
        services.AddHttpClient<IContactNotifier, TelegramContactNotifier>((sp, client) =>
        {
            client.BaseAddress = new Uri(sp.GetRequiredService<IOptions<TelegramOptions>>().Value.ApiUrl);

            // Short on purpose. This call sits between a delivered enquiry and the visitor's
            // answer, so a Telegram that is merely slow must not hold the response open. Now the
            // outer backstop for a pipeline that decides in 8s and 9s.
            client.Timeout = HttpBackstopTimeouts.Telegram;
        })
            // No retry stage. sendMessage is a POST with no idempotency key, so a repeat after a
            // lost response posts the enquiry to the operator's chat twice -- and the mail has
            // already delivered it, so the duplicate buys nothing.
            .AddNoRetryResilience(o => o.Telegram);

        // Reports
        services.AddScoped<Innovayse.Hostpanel.Application.Common.Interfaces.ISslMonitoringService, Innovayse.Hostpanel.Infrastructure.Monitoring.SslMonitoringService>();
        services.AddScoped<Innovayse.Hostpanel.Application.Common.Interfaces.IDiskUsageService, Innovayse.Hostpanel.Infrastructure.Monitoring.DiskUsageService>();

        // Notifications
        // The Smtp section as a whole is still deliberately not checked for completeness here,
        // unlike the integration options above. Every deployed tier fills it from a different
        // overlay and none of them fills all of it, and no tier configures the Notifications
        // section at all. Refusing a partly filled section would refuse to start the production
        // API, so a missing host or port surfaces where the mail is actually sent instead.
        //
        // Encryption is the one value that cannot wait for that, which is why this registration
        // now validates at all. It replaced a UseSsl bool that was allowed to be absent only
        // because the port silently decided in its place; with the guess gone, an unset enum
        // would take its zero value -- None -- and hand the relay's password to the network in
        // the clear on a host that looks healthy. Absent is refused here, by name. A value that
        // is not tls/ssl/none never reaches this check: the binder fails on it first, and
        // ValidateOnStart is what drags that failure forward from the first mail send to boot.
        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.SectionName))
            .Validate(
                o => o.HasEncryption,
                $"{SmtpOptions.SectionName}:{nameof(SmtpOptions.Encryption)} is required and must "
                    + "be one of tls (STARTTLS, conventionally port 587), ssl (implicit TLS, "
                    + "conventionally port 465) or none (a local mail catcher). Set "
                    + "SMTP_ENCRYPTION in this deployment's .env.")
            .ValidateOnStart();
        services.AddOptions<NotificationOptions>()
            .Bind(configuration.GetSection(NotificationOptions.SectionName))
            .ValidateOnStart();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        // IEmailTemplateRepository is gone: every consumer (LocalAuthController,
        // SendPasswordResetHandler, PasswordResetTemplateSeeder) now reads and writes through
        // IHostpanelDbContext (A6 Task 5).
        services.AddScoped<TemplateRenderer>();
        // The brand every mail is rendered with, read from the portal.* settings on each render
        // so an operator's change in the admin panel reaches the next mail without a restart.
        services.AddScoped<IEmailBrandingProvider, SettingsEmailBrandingProvider>();

        // Settings — ISettingRepository is gone: every consumer now reads and writes through
        // IHostpanelDbContext (A6 Task 5).

        // Servers — IServerRepository and IServerGroupRepository are gone: every consumer now
        // reads and writes through IHostpanelDbContext (A6 Task 5).
        services.AddScoped<IServerConnectionTester, ServerConnectionTester>();
        services.AddScoped<Innovayse.Hostpanel.Application.Common.Interfaces.IServerSelector, Innovayse.Hostpanel.Application.Admin.Servers.Common.ServerSelector>();

        // CWP API client.
        //
        // No retry stage and no breaker. Every CWP call is a form POST and the operation lives in
        // the body's `action` field, not in the method or the path: create, suspend, unsuspend and
        // terminate all post to /v1/account, and so does the account listing behind the
        // server-info screen. A predicate cannot separate the read from the writes without
        // re-reading a consumed request body, and guessing wrong re-creates a hosting account.
        // The breaker is off because the server is a per-call argument rather than a base
        // address, so one dead node would open it for every healthy one.
        services.AddHttpClient<Innovayse.SDK.Plugins.IHostingPanelApiClient, Innovayse.Providers.CWP.CwpApiClient>(client =>
        {
            client.Timeout = HttpBackstopTimeouts.Cwp;
        })
            .AddNoRetryResilience(o => o.Cwp);
        // Named twin of the typed registration above, for the same reason the CWP7 pair exists:
        // CwpProvisioningProvider builds its own CwpApiClient (the interface exposes only the
        // server-info call) and resolves this client by name. Before it existed the provider used
        // a bare `new HttpClient()`, so the provisioning path -- the one that matters most -- ran
        // with no timeout and outside the no-retry pipeline.
        services.AddHttpClient(Innovayse.Providers.CWP.CwpApiClient.HttpClientName, client =>
        {
            client.Timeout = HttpBackstopTimeouts.Cwp;
        })
            .AddNoRetryResilience(o => o.Cwp);

        // CWP7 API client (named + typed so the factory can also resolve it by name).
        // Both registrations carry the same profile for the same two reasons as CWP above; the
        // named one is what ProvisioningProviderFactory resolves per server, so leaving it out
        // would have left the provisioning path -- the one that matters most -- uncovered.
        services.AddHttpClient(HttpClientNames.Cwp7, client =>
        {
            client.Timeout = HttpBackstopTimeouts.Cwp7;
        })
            .AddNoRetryResilience(o => o.Cwp7);
        services.AddHttpClient<Innovayse.SDK.Plugins.IHostingPanelV7ApiClient, Innovayse.Providers.CWP7.Cwp7ApiClient>(client =>
        {
            client.Timeout = HttpBackstopTimeouts.Cwp7;
        })
            .AddNoRetryResilience(o => o.Cwp7);

        // Migration
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Pull.MigrationPullWorker>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.MigrationRecordPager>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.MigrationClientResolver>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.MigrationLogWriter>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.MigrationProductLookup>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.ClientsImporter>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.ProductsImporter>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.InvoicesImporter>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.ServicesImporter>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.DomainsImporter>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.TicketsImporter>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.OrdersImporter>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.TransactionsImporter>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.QuotesImporter>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.KnowledgebaseImporter>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.ContactsImporter>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.TicketRepliesImporter>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.AnnouncementsImporter>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.DownloadsImporter>();
        services.AddScoped<Innovayse.Hostpanel.Application.Migration.Importers.NetworkIssuesImporter>();
        services.AddScoped<IMigrationSource, MigrationSourceClient>();
        // The one client where the POST is misleading in the safe direction: the pull protocol
        // posts a signed payload and answers with data, so ping, totals and a page of records all
        // read and nothing on the far side is created or spent. Retrying everything is therefore
        // correct here. No breaker -- the source URL is a per-job argument, so one bad install
        // would fail jobs against every other.
        services.AddHttpClient(HttpClientNames.Migration, client =>
        {
            client.Timeout = HttpBackstopTimeouts.Migration;
        })
            .AddReadOnlyResilience(o => o.Migration);

        // Domains
        services.AddScoped<IRegistrarProvider, NameAmRegistrarProvider>();
        services.AddScoped<IRegistrarProviderFactory, RegistrarProviderFactory>();
        services.AddScoped<Innovayse.Hostpanel.Application.Domains.Registrar.RegistrarProviderResolver>();
        services.AddScoped<NameAmRegistrarProvider>();
        services.AddScoped<IIntegrationConnectionProbe, NameAmConnectionProbe>();
        services.AddScoped<NamecheapRegistrarProvider>();
        // Reads and purchases on the same client, so the predicate has to tell them apart rather
        // than the client being retried wholesale. GET and PUT are repeated -- Name.am's PUT is a
        // whole-resource update of nameservers, contacts or the lock -- and POST only for the
        // availability check and the login, never for /client/carts/purchase, which registers,
        // transfers or renews a domain and bills for it.
        services.AddHttpClient<NameAmClient>(client =>
        {
            client.Timeout = HttpBackstopTimeouts.Registrar;
        })
            .AddNameAmResilience();
        services.AddOptions<NameAmOptions>()
            .Bind(configuration.GetSection(NameAmOptions.SectionName))
            .Validate(
                o => o.IsUsable,
                $"{NameAmOptions.SectionName} is partly configured: Email and Password must either "
                    + "both be set or the whole section left unset.")
            .ValidateOnStart();

        // Namecheap (kept for reference / future multi-registrar support)
        // Every Namecheap call is a GET to the same URL, so the method says nothing and the
        // operation is the Command query parameter -- which ranges from namecheap.domains.check
        // to namecheap.domains.create on the same verb. The predicate reads that parameter and
        // repeats only the lookups; anything it does not recognise counts as a write.
        services.AddHttpClient<NamecheapClient>(client =>
        {
            client.Timeout = HttpBackstopTimeouts.Registrar;
        })
            .AddNamecheapResilience();
        services.AddOptions<NamecheapOptions>()
            .Bind(configuration.GetSection(NamecheapOptions.SectionName))
            .Validate(
                o => o.IsUsable,
                $"{NamecheapOptions.SectionName} is partly configured: ApiUser, ApiKey and ApiUrl "
                    + "must either all be set or the whole section left unset.")
            .ValidateOnStart();

        var pluginsRoot = Path.Combine(AppContext.BaseDirectory, "plugins");
        PluginLoader.DiscoverAndRegister(services, pluginsRoot, loggerFactory);
        services.AddSingleton<IPluginRegistry>(sp => sp.GetRequiredService<PluginRegistry>());

        // Public demo. Last on purpose: when Demo:Enabled is true this swaps the outward ports
        // registered above (payment, registrar, provisioning, rates, monitoring) for offline fakes,
        // and a Replace can only replace what is already there. Email is not among them: it stays
        // on the configured relay so a demo can show real mail in a catcher.
        services.AddDemoMode(configuration, ownsItsUsers);

        return services;
    }
}
