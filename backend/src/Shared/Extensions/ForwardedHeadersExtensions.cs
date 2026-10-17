using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Innovayse.Hostpanel.Shared.Settings;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using AspNetForwardedHeadersOptions = Microsoft.AspNetCore.Builder.ForwardedHeadersOptions;
using AspNetIpNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace Innovayse.Hostpanel.Shared.Extensions;

/// <summary>
/// Forwarded headers from configured proxies only. After <c>UseForwardedHeaders()</c>,
/// <see cref="ConnectionInfo.RemoteIpAddress"/> is the real client when the peer is a trusted
/// proxy and the TCP peer otherwise — the only client IP the rate limiter reads.
/// </summary>
public static class ForwardedHeadersExtensions
{
    /// <summary>
    /// Binds <see cref="ForwardedHeadersOptions"/>, validates every entry at startup, and configures
    /// <see cref="AspNetForwardedHeadersOptions"/> with exactly those proxies. The framework's loopback
    /// defaults are cleared so nothing is trusted implicitly.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">The root configuration.</param>
    /// <returns>Service collection for chaining.</returns>
    public static IServiceCollection AddTrustedForwardedHeaders(
        this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<ForwardedHeadersOptions>()
            .Bind(configuration.GetSection(ForwardedHeadersOptions.SectionName))
            .Validate(
                s => s.KnownProxies.All(p => IPAddress.TryParse(p, out _)),
                "ForwardedHeaders:KnownProxies must contain IP addresses only.")
            .Validate(
                s => s.KnownNetworks.All(n => TryParseNetwork(n, out _)),
                "ForwardedHeaders:KnownNetworks must contain CIDR networks only.")
            .ValidateOnStart();

        services.AddOptions<AspNetForwardedHeadersOptions>()
            .Configure<IOptions<ForwardedHeadersOptions>>((options, trusted) =>
            {
                // The defaults trust loopback; behind a container network that could be anything.
                options.KnownProxies.Clear();
                options.KnownNetworks.Clear();

                // With both lists empty the middleware skips the known-address check and would
                // trust X-Forwarded-For from any caller, so no proxy configured = no header processed.
                if (trusted.Value.KnownProxies.Length == 0 && trusted.Value.KnownNetworks.Length == 0)
                {
                    options.ForwardedHeaders = ForwardedHeaders.None;
                    return;
                }

                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                foreach (var proxy in trusted.Value.KnownProxies)
                {
                    options.KnownProxies.Add(IPAddress.Parse(proxy));
                }

                foreach (var network in trusted.Value.KnownNetworks)
                {
                    if (TryParseNetwork(network, out var parsed))
                    {
                        options.KnownNetworks.Add(parsed);
                    }
                }
            });

        return services;
    }

    /// <summary>Parses <c>address/prefix</c> into a network.</summary>
    /// <param name="value">The CIDR text.</param>
    /// <param name="network">The parsed network; null when the text is not valid.</param>
    /// <returns>Whether the text was a valid CIDR network.</returns>
    private static bool TryParseNetwork(string value, out AspNetIpNetwork network)
    {
        network = null!;
        var slash = value.IndexOf('/', StringComparison.Ordinal);
        if (slash <= 0)
        {
            return false;
        }

        if (!IPAddress.TryParse(value.AsSpan(0, slash), out var prefix)
            || !int.TryParse(value.AsSpan(slash + 1), NumberStyles.None, CultureInfo.InvariantCulture, out var length))
        {
            return false;
        }

        var max = prefix.AddressFamily == AddressFamily.InterNetworkV6 ? 128 : 32;
        if (length > max)
        {
            return false;
        }

        network = new AspNetIpNetwork(prefix, length);
        return true;
    }
}
