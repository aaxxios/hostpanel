using Innovayse.Hostpanel.Infrastructure.Security;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An <see cref="IHttpClientFactory"/> whose every client dials through an <see cref="OutboundConnector"/>,
/// the way the production clients registered with an address guard do.
/// </summary>
/// <param name="connector">The connector every connection goes through.</param>
public sealed class ConnectorHttpClientFactory(OutboundConnector connector) : IHttpClientFactory
{
    /// <inheritdoc/>
    public HttpClient CreateClient(string name)
    {
        return new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false, ConnectCallback = connector.ConnectAsync });
    }
}
