namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.TestServerSshAgent;

/// <summary>Outcome of an agent health check.</summary>
/// <param name="IsHealthy">Whether the agent answered.</param>
/// <param name="AgentVersion">The agent's version when it answered.</param>
/// <param name="Error">
/// The reason code when it is not healthy; never host detail. <c>binding_mismatch</c> means the agent
/// answering was installed for another server record.
/// </param>
/// <param name="Provider">The provider profile the agent runs, when it answered.</param>
public sealed record TestServerSshAgentResultDto(bool IsHealthy, string? AgentVersion, string? Error, string? Provider = null);
