using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Infrastructure.Security.Extensions;

/// <summary>
/// Puts an HTTP client that dials admin-entered hosts behind <see cref="OutboundConnector"/>.
/// </summary>
public static class OutboundHttpClientBuilderExtensions
{
    /// <summary>
    /// Replaces the client's primary handler with one whose every connection goes through
    /// <see cref="OutboundConnector.ConnectAsync"/>, with the connector's connect timeout. A redirect
    /// is dialled through the same callback, so following one cannot reach a refused address.
    /// </summary>
    /// <param name="builder">The client registration.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IHttpClientBuilder AddOutboundAddressGuard(this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.ConfigurePrimaryHttpMessageHandler(sp =>
        {
            var connector = sp.GetRequiredService<OutboundConnector>();
            return new SocketsHttpHandler
            {
                ConnectTimeout = connector.ConnectTimeout,
                ConnectCallback = connector.ConnectAsync,
            };
        });
    }
}
