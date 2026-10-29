using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.SDK.Plugins;
using Innovayse.SDK.Plugins.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Infrastructure.Plugins;

/// <summary>
/// Resolves a loaded payment plugin into a ready-to-use <see cref="IPaymentPlugin"/> instance,
/// gated by admin-configured settings rather than static appsettings. Settings are read fresh
/// on every call so that credential edits made in the admin panel take effect immediately,
/// without an API restart.
/// </summary>
/// <remarks>
/// A plugin sees only its own configuration: the host's <c>integration:{id}</c> section with the
/// admin-saved <c>integration:{id}:*</c> settings layered over it. Nothing else from the host's
/// configuration (connection strings, signing keys, other integrations) is handed over. The SDK's
/// base classes read <c>integration:{id}:{key}</c> by its full path, so the plugin receives a
/// configuration holding just that section under its full path rather than a relative
/// <see cref="IConfigurationSection"/>, which would make every such read miss.
/// </remarks>
public sealed class PaymentPluginResolver : IPaymentPluginResolver
{
    /// <summary>The data context, used to read the current settings snapshot.</summary>
    private readonly AppDbContext _db;

    /// <summary>Decrypts the stored secret fields before they reach the plugin.</summary>
    private readonly ISecretSettingCipher _cipher;

    /// <summary>Registry of plugins loaded at startup.</summary>
    private readonly PluginRegistry _registry;

    /// <summary>
    /// Service provider used to construct the plugin instance with its other dependencies. The plugin type is
    /// loaded from a folder at run time, so its constructor is only known then; that is why Wolverine takes this
    /// port from the container rather than building it (see <c>MessagingRegistration</c>).
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>Host application configuration; only the plugin's own section is ever read from it.</summary>
    private readonly IConfiguration _hostConfiguration;

    /// <summary>Structured logger: a null result is ambiguous to callers, so each rejection gate logs why.</summary>
    private readonly ILogger<PaymentPluginResolver> _logger;

    /// <summary>Initializes a new instance of the <see cref="PaymentPluginResolver"/> class.</summary>
    /// <param name="db">The data context, used to read the current settings snapshot.</param>
    /// <param name="cipher">Decrypts the stored secret fields before they reach the plugin.</param>
    /// <param name="registry">Registry of plugins loaded at startup.</param>
    /// <param name="serviceProvider">Service provider used to construct the plugin instance with its other dependencies.</param>
    /// <param name="hostConfiguration">Host application configuration; only the plugin's own section is ever read from it.</param>
    /// <param name="logger">Structured logger: a null result is ambiguous to callers, so each rejection gate logs why.</param>
    public PaymentPluginResolver(AppDbContext db, ISecretSettingCipher cipher, PluginRegistry registry, IServiceProvider serviceProvider, IConfiguration hostConfiguration, ILogger<PaymentPluginResolver> logger)
    {
        _db = db;
        _cipher = cipher;
        _registry = registry;
        _serviceProvider = serviceProvider;
        _hostConfiguration = hostConfiguration;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IPaymentPlugin?> ResolveAsync(string module, CancellationToken ct)
    {
        var plugin = _registry.Find(module);
        if (plugin is null || plugin.Manifest.Type != PluginType.Payment)
        {
            _logger.LogWarning(
                "Payment plugin '{Module}' is not installed or is not a payment-type plugin.", module);
            return null;
        }

        // Manifest.Type == Payment is only a JSON claim the plugin author made; it says nothing
        // about the CLR type actually loaded. Verify the implementation really implements
        // IPaymentPlugin before trusting it far enough to construct and cast it below: a
        // mistyped third-party manifest must return the documented null, not an
        // InvalidCastException thrown out of the payment path.
        if (!typeof(IPaymentPlugin).IsAssignableFrom(plugin.ImplementationType))
        {
            _logger.LogWarning(
                "Payment plugin '{Module}' declares Type={ManifestType} but its implementation " +
                "type '{ImplementationType}' does not implement {InterfaceType}.",
                module, plugin.Manifest.Type, plugin.ImplementationType, nameof(IPaymentPlugin));
            return null;
        }

        // The host's own section for this plugin, keys kept under their full path.
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in _hostConfiguration.GetSection(IntegrationSettingKeys.Section(module)).AsEnumerable())
        {
            if (value is not null)
            {
                values[key] = value;
            }
        }

        // Only this plugin's rows, filtered in SQL; read-only, so no tracking is needed.
        var prefix = IntegrationSettingKeys.Prefix(module);
        var settings = await _db.Settings.AsNoTracking()
            .Where(s => s.Key.StartsWith(prefix))
            .ToListAsync(ct);
        foreach (var setting in settings)
        {
            values[setting.Key] = _cipher.Reveal(setting.Value);
        }

        if (!values.TryGetValue(IntegrationSettingKeys.EnabledKey(module), out var isEnabled)
            || !string.Equals(isEnabled, "true", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Payment plugin '{Module}' is disabled.", module);
            return null;
        }

        foreach (var field in plugin.Manifest.Fields)
        {
            if (!field.Required)
            {
                continue;
            }

            if (!values.TryGetValue(IntegrationSettingKeys.FieldKey(module, field.Key), out var fieldValue)
                || string.IsNullOrWhiteSpace(fieldValue))
            {
                _logger.LogWarning(
                    "Payment plugin '{Module}' is missing required setting '{Field}'.", module, field.Key);
                return null;
            }
        }

        var composed = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return (IPaymentPlugin)ActivatorUtilities.CreateInstance(
            _serviceProvider, plugin.ImplementationType, (IConfiguration)composed);
    }
}
