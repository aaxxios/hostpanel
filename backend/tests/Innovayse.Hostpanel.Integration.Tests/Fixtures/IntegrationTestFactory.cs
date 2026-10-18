using System.Net.Http.Json;
using System.Text.Json;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Infrastructure.Auth;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Domains;
using Innovayse.Hostpanel.Integration.Tests.Provisioning;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;
/// <summary>
/// Spins up a real PostgreSQL container via Testcontainers and hosts the API
/// in-process using <see cref="WebApplicationFactory{TEntryPoint}"/>.
/// Share across test classes via <see cref="IClassFixture{T}"/>.
/// </summary>
public sealed class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>
    /// AES-256 key for the test host: 32 bytes, Base64. Fixed rather than generated so a
    /// failing run can be reproduced, and harmless in the open because the database it
    /// protects is a throwaway container that lives for the length of one test class.
    /// </summary>
    private const string TestEncryptionKey = "MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY=";

    /// <summary>PostgreSQL container started before each test class.</summary>
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("innovayse_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // UseSetting, not ConfigureAppConfiguration, for these two. Program.cs is
        // top-level code: it reads Auth:Mode and passes the configuration to
        // AddInfrastructure — which reads EncryptionKey — while composing the builder,
        // and ConfigureAppConfiguration's sources are not added until builder.Build().
        // Set the late way, both keys were simply absent at the moment they were read:
        // AddInnovayseAuth ran (mode was not "local") and threw "Auth:AppName is
        // required", the catch in Program.cs logged it, and all 41 tests reported "The
        // entry point exited without ever building an IHost". UseSetting writes into the
        // host configuration, which exists before the first line of Program.cs runs.
        //
        // Auth:Mode: every test here creates its users through POST /api/auth/register —
        // the only user-creation path in the suite — and LocalAuthController 404s that
        // action unless the mode is local (sso by default, per docker-compose.yml's
        // AUTH_MODE:-sso).
        //
        // EncryptionKey: AddInfrastructure refuses to start outside Development without
        // one, because an absent key silently wrote server credentials and client-service
        // passwords to the database in plain text. A real key rather than an exemption,
        // so these tests exercise the encrypting EF converters that production uses.
        builder.UseSetting("Auth:Mode", "local");
        builder.UseSetting("EncryptionKey", TestEncryptionKey);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Read when the DbContext is first resolved, long after Build(), so this one
            // is safe to add here — and it has to be, because the container it names is
            // not started until InitializeAsync runs.
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString()
            });
        });

        // Suppress Windows EventLog provider to prevent ObjectDisposedException on teardown
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureServices(services =>
        {
            // Replace the real registrar provider with a no-op stub so tests never hit the network.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IRegistrarProvider));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddScoped<IRegistrarProvider, StubRegistrarProvider>();

            // Replace real control panel provisioning provider with a no-op stub.
            var provisioningDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IProvisioningProvider));
            if (provisioningDescriptor is not null)
            {
                services.Remove(provisioningDescriptor);
            }

            services.AddScoped<IProvisioningProvider, StubProvisioningProvider>();

            // And the factory that actually gets asked for one. ProvisionServiceHandler
            // never resolves IProvisioningProvider — it calls
            // IProvisioningProviderFactory.CreateFor(server), and the real implementation
            // builds a hosting panel (v7) client aimed at the server row's hostname. Stubbing only the
            // interface above left every provisioning test making a real network call to
            // a host that does not exist, which came back as 400 from the endpoint.
            var providerFactoryDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IProvisioningProviderFactory));
            if (providerFactoryDescriptor is not null)
            {
                services.Remove(providerFactoryDescriptor);
            }

            services.AddScoped<IProvisioningProviderFactory, StubProvisioningProviderFactory>();

            // Registration now sends the verification mail, so a host that kept the SMTP sender would
            // wait on a relay that does not exist for every account a test creates. Tests that read the
            // mail back derive a host with their own FakeEmailSender (EventHandlerTestHost.WithFakeRelay).
            var emailSenderDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IEmailSender));
            if (emailSenderDescriptor is not null)
            {
                services.Remove(emailSenderDescriptor);
            }

            services.AddSingleton<IEmailSender, FakeEmailSender>();
        });
    }

    /// <summary>
    /// Starts the PostgreSQL container, applies EF Core migrations, and seeds the roles and
    /// storefront settings Program.cs would otherwise seed on a real startup outside Testing.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Apply EF Core migrations to test database
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        // Seed roles after migration
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { Roles.Admin, Roles.Reseller, Roles.Client })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Storefront settings — Program.cs seeds these on every real startup except
        // Testing (the block this factory's role-seeding above also stands in for), so
        // without this call the settings table is permanently empty in every Integration
        // test and GetSettings/GetPublicSettings/UpdateSetting have nothing to read or
        // write against.
        await Innovayse.Hostpanel.Application.Admin.Seeding.PortalSettingsSeeder.EnsureSeededAsync(
            scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>(),
            scope.ServiceProvider.GetRequiredService<Innovayse.Hostpanel.Application.Common.Interfaces.IUnitOfWork>());
    }

    /// <summary>
    /// Releases the API host first (through <see cref="WebApplicationFactory{TEntryPoint}"/>, which also
    /// disposes every host derived with <c>WithWebHostBuilder</c>: service provider, hosted services,
    /// message bus, Npgsql data sources and EF caches), then drops the pooled connections to the
    /// container and finally disposes the container itself. This is the <see cref="IAsyncDisposable"/>
    /// override, so <c>await using var factory = new IntegrationTestFactory()</c> -- which the tests that
    /// build a factory of their own rely on -- releases the container as well; before, only the
    /// <see cref="IAsyncLifetime"/> path did, and every such test left a PostgreSQL container running.
    /// </summary>
    /// <returns>A task that completes when the host, the pools and the container are released.</returns>
    public override async ValueTask DisposeAsync()
    {
        try
        {
            await base.DisposeAsync();
        }
        finally
        {
            // Npgsql pools are keyed by connection string and each class gets a container on its own
            // port, so every pool left behind belongs to a server about to disappear.
            NpgsqlConnection.ClearAllPools();
            await _postgres.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>Disposes through the <see cref="IAsyncDisposable"/> path; xUnit calls this one.</summary>
    /// <returns>A task that completes when the host, the pools and the container are released.</returns>
    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();

    /// <summary>
    /// Creates a seeded admin user (if not yet created) and returns a valid JWT access token.
    /// The admin user is assigned the Admin role directly via ASP.NET Core Identity.
    /// </summary>
    /// <returns>JWT access token string for the admin user.</returns>
    public async Task<string> GetAdminTokenAsync()
    {
        const string AdminEmail = "admin-seed@innovayse.test";
        const string AdminPassword = "Admin@Seed123!";

        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var existing = await userManager.FindByEmailAsync(AdminEmail);
        if (existing is null)
        {
            var user = new AppUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                FirstName = "Seeded",
                LastName = "Admin",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, AdminPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create seeded admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await userManager.AddToRoleAsync(user, Roles.Admin);
        }

        return await GetTokenAsync(AdminEmail, AdminPassword);
    }

    /// <summary>
    /// Logs in an existing user and returns the JWT access token.
    /// </summary>
    /// <param name="email">User email address.</param>
    /// <param name="password">User password.</param>
    /// <returns>JWT access token string.</returns>
    public async Task<string> GetClientTokenAsync(string email, string password) =>
        await GetTokenAsync(email, password);

    /// <summary>
    /// Posts to /api/auth/login and extracts the accessToken from the response.
    /// </summary>
    /// <param name="email">User email.</param>
    /// <param name="password">User password.</param>
    /// <returns>JWT access token string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when login fails or token is missing.</exception>
    private async Task<string> GetTokenAsync(string email, string password)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Login failed for {email}: {response.StatusCode} — {body}");
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("accessToken").GetString()
            ?? throw new InvalidOperationException("accessToken missing from login response.");
    }
}
