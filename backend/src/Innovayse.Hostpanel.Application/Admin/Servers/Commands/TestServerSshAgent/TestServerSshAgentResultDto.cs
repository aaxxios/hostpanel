namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.TestServerSshAgent;

/// <summary>Outcome of an agent health check.</summary>
/// <param name="IsHealthy">Whether the agent answered.</param>
/// <param name="AgentVersion">The agent's version when it answered.</param>
/// <param name="Error">The provider's reason code when it did not; never host detail.</param>
public sealed record TestServerSshAgentResultDto(bool IsHealthy, string? AgentVersion, string? Error);
