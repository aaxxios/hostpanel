using System.Net;
using System.Text;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An HTTP handler that answers every request with HTTP 200 and the same JSON body, without a
/// network round trip.
/// </summary>
/// <remarks>
/// For adapters whose base URL is always HTTPS on a panel's own port, which the plain HTTP
/// <see cref="StubHttpServer"/> cannot stand in for. It fakes the remote panel only, never our code.
/// </remarks>
public sealed class FixedReplyHttpMessageHandler : HttpMessageHandler
{
    /// <summary>The JSON body every request is answered with.</summary>
    private readonly string _body;

    /// <summary>Initializes the handler.</summary>
    /// <param name="body">The JSON body every request is answered with.</param>
    public FixedReplyHttpMessageHandler(string body)
    {
        _body = body;
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(_body, Encoding.UTF8, "application/json"),
        });
}
