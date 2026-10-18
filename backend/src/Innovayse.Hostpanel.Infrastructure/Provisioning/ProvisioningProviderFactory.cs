using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Infrastructure.Integrations.CPanel;
using Innovayse.Hostpanel.Infrastructure.Provisioning.Settings;
using Innovayse.Providers.CWP7;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace Innovayse.Hostpanel.Infrastructure.Provisioning;
/// <summary>
/// Creates <see cref="IProvisioningProvider"/> instances configured for a specific server.
/// Resolves the correct provider implementation based on the server's <see cref="Server.Module"/>.
/// </summary>
public sealed class ProvisioningProviderFactory : IProvisioningProviderFactory
{
    /// <summary>Factory for creating HTTP clients for provider API calls.</summary>
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>Factory for creating typed loggers for each provider.</summary>
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>Provisioning behaviour switches, notably the development fake-provider flag.</summary>
    private readonly IOptions<ProvisioningOptions> _options;

    /// <summary>Initializes a new instance of the <see cref="ProvisioningProviderFactory"/> class.</summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients for provider API calls.</param>
    /// <param name="loggerFactory">Factory for creating typed loggers for each provider.</param>
    /// <param name="options">Provisioning behaviour switches, notably the development fake-provider flag.</param>
    public ProvisioningProviderFactory(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory, IOptions<ProvisioningOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
        _options = options;
    }

    /// <summary>Default CWP7 API port.</summary>
    private const int DefaultCwp7Port = 2304;

    /// <summary>
    /// Creates a provisioning provider configured with the credentials of the given server.
    /// </summary>
    /// <param name="server">The server to create the provider for.</param>
    /// <returns>A configured <see cref="IProvisioningProvider"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown for unsupported server modules.</exception>
    /// <exception cref="InvalidOperationException">Thrown when required credentials are missing.</exception>
    public IProvisioningProvider CreateFor(Server server)
    {
        // Development short-circuit: with no live control panel to call, return the in-process
        // no-op provider so the setup flow completes. Guarded by config that defaults off, so a
        // real environment never reaches this and always builds a provider that provisions for
        // real. Server selection still ran before this, so the flow is exercised end to end bar
        // the outbound HTTP call itself.
        if (_options.Value.UseFakeProvider)
        {
            return new NullCPanelProvisioningProvider(
                _loggerFactory.CreateLogger<NullCPanelProvisioningProvider>());
        }

        return server.Module switch
        {
            ServerModule.Cwp7 => CreateCwp7Provider(server),
            _ => throw new NotSupportedException($"No provisioning provider for module '{server.Module}'. Only CWP7 is currently supported via the factory."),
        };
    }

    /// <summary>
    /// Creates a CWP7 provider for the given server.
    /// </summary>
    /// <param name="server">The CWP7 server.</param>
    /// <returns>A configured <see cref="Cwp7ProvisioningProvider"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the server has no usable access hash — null, empty or whitespace.
    /// </exception>
    private Cwp7ProvisioningProvider CreateCwp7Provider(Server server)
    {
        // Whitespace, not just null: a server row saved with the access-hash field cleared holds
        // an empty string, and that used to build a provider with an empty API key that failed
        // later, at the provider, instead of here with the message this line already writes.
        var apiKey = server.AccessHash;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException($"Server '{server.Name}' (id={server.Id}) has no access hash configured.");
        }

        var host = $"https://{server.Hostname}:{DefaultCwp7Port}";
        var serverIp = server.IpAddress ?? server.Hostname;

        var httpClient = _httpClientFactory.CreateClient("Cwp7");

        var client = new Cwp7ApiClient(httpClient, _loggerFactory.CreateLogger<Cwp7ApiClient>());

        return new Cwp7ProvisioningProvider(
            host, apiKey, serverIp, client,
            _loggerFactory.CreateLogger<Cwp7ProvisioningProvider>());
    }
}
