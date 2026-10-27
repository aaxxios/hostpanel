namespace Innovayse.Hostpanel.Application.Admin.Servers.Common;

/// <summary>A server's SSH key management settings for the admin panel. Never carries the private key.</summary>
/// <param name="ServerId">The server.</param>
/// <param name="IsSupported">Whether the server's module has an SSH key implementation.</param>
/// <param name="IsEnabled">Whether customers may manage keys.</param>
/// <param name="SshHostname">Customer-facing hostname.</param>
/// <param name="SshPort">Customer-facing port.</param>
/// <param name="AgentHost">Agent host.</param>
/// <param name="AgentPort">Agent SSH port.</param>
/// <param name="AgentUsername">Management user.</param>
/// <param name="HasAgentPrivateKey">Whether a private key is stored.</param>
/// <param name="AgentHostKeyFingerprint">Pinned host key fingerprint.</param>
/// <param name="ConnectTimeoutSeconds">Connect timeout.</param>
/// <param name="OperationTimeoutSeconds">Operation timeout.</param>
/// <param name="IsHealthy">Last health check outcome.</param>
/// <param name="LastHealthCheckAt">When it ran.</param>
public sealed record ServerSshSettingsDto(
    int ServerId,
    bool IsSupported,
    bool IsEnabled,
    string SshHostname,
    int SshPort,
    string AgentHost,
    int AgentPort,
    string AgentUsername,
    bool HasAgentPrivateKey,
    string AgentHostKeyFingerprint,
    int ConnectTimeoutSeconds,
    int OperationTimeoutSeconds,
    bool? IsHealthy,
    DateTimeOffset? LastHealthCheckAt);
