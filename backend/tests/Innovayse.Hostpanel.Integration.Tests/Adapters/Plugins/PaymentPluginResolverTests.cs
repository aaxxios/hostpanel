using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Infrastructure.Plugins;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.SDK.Plugins;
using Innovayse.SDK.Plugins.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Plugins;

/// <summary>
/// <see cref="PaymentPluginResolver"/> settings gating and instantiation, against Postgres.
/// </summary>
/// <remarks>
/// The resolver reads <c>AppDbContext</c>, so these tests run against a real database. Every test names its own module id (a fresh
/// <see cref="Guid"/> per call) so the settings each test writes never collide with another
/// test's rows under <c>Settings.Key</c>'s unique index.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class PaymentPluginResolverTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Minimal payment plugin used as the registered implementation type.</summary>
    public sealed class FakePaymentPlugin : IPaymentPlugin
    {
        /// <summary>Initializes the fake with the composed configuration for assertions.</summary>
        /// <param name="configuration">The configuration the resolver composed.</param>
        public FakePaymentPlugin(IConfiguration configuration) => Configuration = configuration;

        /// <summary>Gets the configuration the resolver passed in.</summary>
        public IConfiguration Configuration { get; }

        /// <inheritdoc/>
        public string CurrencyCode => "840";

        /// <inheritdoc/>
        public Task<PaymentSession> CreatePaymentAsync(PaymentRequest request, CancellationToken ct)
            => throw new NotSupportedException();

        /// <inheritdoc/>
        public Task<GatewayPaymentStatus> GetStatusAsync(string gatewayOrderId, CancellationToken ct)
            => throw new NotSupportedException();

        /// <inheritdoc/>
        public Task<string> RefundAsync(string gatewayOrderId, long amountMinor, CancellationToken ct)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// Implementation type that deliberately does not implement <see cref="IPaymentPlugin"/>,
    /// standing in for a third-party plugin whose manifest mistakenly (or maliciously) claims
    /// <see cref="PluginType.Payment"/> for an entry point that isn't one.
    /// </summary>
    public sealed class MistypedManifestPlugin
    {
    }

    /// <summary>Builds a manifest for the fake plugin under the given module id.</summary>
    /// <param name="module">The module id to register the manifest under.</param>
    /// <param name="type">The manifest's declared plugin type.</param>
    /// <returns>The manifest.</returns>
    private static PluginManifest Manifest(string module, PluginType type = PluginType.Payment) => new()
    {
        Id = module,
        Name = "Fake Pay",
        Version = "1.0.0",
        Author = "Innovayse",
        Description = "Fake payment plugin for tests.",
        Type = type,
        Category = "Payment",
        EntryPoint = typeof(FakePaymentPlugin).FullName!,
        SdkVersion = "1.0.0",
        Fields =
        [
            new PluginField { Key = "api_key", Label = "API Key", Type = "secret", Required = true },
        ],
    };

    /// <summary>
    /// Writes the given settings rows for the module and builds a resolver over the real database.
    /// </summary>
    /// <remarks>
    /// Takes the caller's own <paramref name="db"/> rather than resolving one from a scope of its
    /// own: a scope disposes its <see cref="AppDbContext"/> when it goes out of scope, and the
    /// returned resolver reads <c>db.Settings</c> again on every <c>ResolveAsync</c> call, well
    /// after this method returns -- so the context has to outlive it.
    /// </remarks>
    /// <param name="db">The data context to seed and to build the resolver over.</param>
    /// <param name="module">The module id the settings and the registered manifest share.</param>
    /// <param name="settings">The setting key/value pairs to seed (full keys, not just the field name).</param>
    /// <param name="pluginType">The manifest's declared plugin type.</param>
    /// <param name="hostConfig">Host configuration layered underneath the settings; empty by default.</param>
    /// <param name="implementationType">The CLR type registered as the plugin's entry point.</param>
    /// <returns>The resolver, ready to call <c>ResolveAsync</c> against.</returns>
    private static async Task<PaymentPluginResolver> CreateResolverAsync(
        AppDbContext db,
        string module,
        Dictionary<string, string> settings,
        PluginType pluginType = PluginType.Payment,
        IConfiguration? hostConfig = null,
        Type? implementationType = null)
    {
        foreach (var (key, value) in settings)
        {
            db.Settings.Add(Setting.Create(key, value, null));
        }

        await db.SaveChangesAsync(CancellationToken.None);

        var registry = new PluginRegistry();
        registry.Register(new LoadedPlugin(Manifest(module, pluginType), implementationType ?? typeof(FakePaymentPlugin)));

        var services = new ServiceCollection().BuildServiceProvider();
        return new PaymentPluginResolver(
            db, registry, services, hostConfig ?? new ConfigurationBuilder().Build(),
            NullLogger<PaymentPluginResolver>.Instance);
    }

    /// <summary>An enabled, fully configured plugin resolves with the composed configuration.</summary>
    [Fact]
    public async Task ResolveAsync_EnabledAndConfigured_ReturnsPluginWithComposedConfig()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var module = $"fake-pay-{Guid.NewGuid():N}";
        var resolver = await CreateResolverAsync(db, module, new()
        {
            [$"integration:{module}:is_enabled"] = "true",
            [$"integration:{module}:api_key"] = "k-123",
        });

        var plugin = await resolver.ResolveAsync(module, CancellationToken.None);

        var fake = Assert.IsType<FakePaymentPlugin>(plugin);
        Assert.Equal("k-123", fake.Configuration[$"integration:{module}:api_key"]);
    }

    /// <summary>A plugin the admin has switched off resolves to nothing, whatever its credentials say.</summary>
    [Fact]
    public async Task ResolveAsync_Disabled_ReturnsNull()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var module = $"fake-pay-{Guid.NewGuid():N}";
        var resolver = await CreateResolverAsync(db, module, new()
        {
            [$"integration:{module}:is_enabled"] = "false",
            [$"integration:{module}:api_key"] = "k-123",
        });

        Assert.Null(await resolver.ResolveAsync(module, CancellationToken.None));
    }

    /// <summary>A plugin missing a required field resolves to nothing.</summary>
    [Fact]
    public async Task ResolveAsync_MissingRequiredField_ReturnsNull()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var module = $"fake-pay-{Guid.NewGuid():N}";
        var resolver = await CreateResolverAsync(db, module, new()
        {
            [$"integration:{module}:is_enabled"] = "true",
        });

        Assert.Null(await resolver.ResolveAsync(module, CancellationToken.None));
    }

    /// <summary>A module nobody registered resolves to nothing.</summary>
    [Fact]
    public async Task ResolveAsync_UnknownModule_ReturnsNull()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var module = $"fake-pay-{Guid.NewGuid():N}";
        var resolver = await CreateResolverAsync(db, module, []);

        Assert.Null(await resolver.ResolveAsync("nope", CancellationToken.None));
    }

    /// <summary>A manifest of the wrong plugin type resolves to nothing even when fully configured.</summary>
    [Fact]
    public async Task ResolveAsync_WrongPluginType_ReturnsNull()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var module = $"fake-pay-{Guid.NewGuid():N}";
        var resolver = await CreateResolverAsync(
            db,
            module,
            new()
            {
                [$"integration:{module}:is_enabled"] = "true",
                [$"integration:{module}:api_key"] = "k-123",
            },
            pluginType: PluginType.Provisioning);

        Assert.Null(await resolver.ResolveAsync(module, CancellationToken.None));
    }

    /// <summary>A missing <c>is_enabled</c> key counts as disabled, not as an error.</summary>
    [Fact]
    public async Task ResolveAsync_IsEnabledKeyAbsent_ReturnsNull()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var module = $"fake-pay-{Guid.NewGuid():N}";
        var resolver = await CreateResolverAsync(db, module, new()
        {
            [$"integration:{module}:api_key"] = "k-123",
        });

        Assert.Null(await resolver.ResolveAsync(module, CancellationToken.None));
    }

    /// <summary>
    /// A manifest's <c>Type == Payment</c> is only a JSON claim; the resolver must verify the CLR
    /// type actually implements <see cref="IPaymentPlugin"/> before casting, and fail closed
    /// (null) rather than let an <see cref="InvalidCastException"/> escape the payment path.
    /// </summary>
    [Fact]
    public async Task ResolveAsync_ManifestClaimsPaymentButImplementationDoesNotImplementIPaymentPlugin_ReturnsNullWithoutThrowing()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var module = $"fake-pay-{Guid.NewGuid():N}";
        var resolver = await CreateResolverAsync(
            db,
            module,
            new()
            {
                [$"integration:{module}:is_enabled"] = "true",
                [$"integration:{module}:api_key"] = "k-123",
            },
            implementationType: typeof(MistypedManifestPlugin));

        var result = await resolver.ResolveAsync(module, CancellationToken.None);

        Assert.Null(result);
    }

    /// <summary>An admin-configured setting overrides the same key in the host's own configuration.</summary>
    [Fact]
    public async Task ResolveAsync_SettingsOverrideHostConfiguration()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var module = $"fake-pay-{Guid.NewGuid():N}";
        var hostConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"integration:{module}:api_key"] = "old",
            })
            .Build();

        var resolver = await CreateResolverAsync(
            db,
            module,
            new()
            {
                [$"integration:{module}:is_enabled"] = "true",
                [$"integration:{module}:api_key"] = "new",
            },
            hostConfig: hostConfig);

        var plugin = await resolver.ResolveAsync(module, CancellationToken.None);

        var fake = Assert.IsType<FakePaymentPlugin>(plugin);
        Assert.Equal("new", fake.Configuration[$"integration:{module}:api_key"]);
    }
}
