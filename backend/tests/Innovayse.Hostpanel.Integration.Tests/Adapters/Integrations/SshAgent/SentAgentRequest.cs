using System.Text.Json.Nodes;
using Innovayse.Hostpanel.Infrastructure.Integrations.SshAgent;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Integrations.SshAgent;

/// <summary>One request the SSH agent provider sent through the transport.</summary>
/// <param name="Endpoint">The agent endpoint the request went to.</param>
/// <param name="Request">The parsed request body.</param>
internal sealed record SentAgentRequest(SshAgentEndpoint Endpoint, JsonObject Request);
