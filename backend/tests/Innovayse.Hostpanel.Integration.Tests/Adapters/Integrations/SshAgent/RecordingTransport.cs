using System.Text.Json.Nodes;
using Innovayse.Hostpanel.Infrastructure.Integrations.SshAgent;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Integrations.SshAgent;

/// <summary>
/// Records requests and answers a fixed response. Its <see cref="SendAsync"/> is handed to the
/// provider as its <see cref="SshAgentSend"/>.
/// </summary>
/// <param name="response">The response to answer.</param>
internal sealed class RecordingTransport(string response)
{
    /// <summary>Gets the requests sent so far, in order.</summary>
    public List<SentAgentRequest> Sent { get; } = [];

    /// <summary>Records the request and answers the fixed response.</summary>
    /// <param name="endpoint">The agent endpoint the request was addressed to.</param>
    /// <param name="requestJson">The request document.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The fixed response.</returns>
    public Task<string> SendAsync(SshAgentEndpoint endpoint, string requestJson, CancellationToken ct)
    {
        Sent.Add(new SentAgentRequest(endpoint, JsonNode.Parse(requestJson)!.AsObject()));
        return Task.FromResult(response);
    }
}
