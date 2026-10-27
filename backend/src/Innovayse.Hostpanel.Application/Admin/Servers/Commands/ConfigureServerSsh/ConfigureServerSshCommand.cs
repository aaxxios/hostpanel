namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.ConfigureServerSsh;

/// <summary>Admin: creates or replaces a server's SSH key management settings.</summary>
/// <param name="ServerId">The server (taken from the route).</param>
/// <param name="IsEnabled">Whether customers may manage keys.</param>
/// <param name="SshHostname">Customer-facing hostname.</param>
/// <param name="SshPort">Customer-facing port.</param>
/// <param name="AgentHost">Agent host.</param>
/// <param name="AgentPort">Agent SSH port.</param>
/// <param name="AgentUsername">Management user.</param>
/// <param name="AgentPrivateKey">New OpenSSH private key, or <see langword="null"/>/empty to keep the stored one.</param>
/// <param name="AgentHostKeyFingerprint">Pinned host key fingerprint, <c>SHA256:…</c>.</param>
/// <param name="ConnectTimeoutSeconds">Connect timeout.</param>
/// <param name="OperationTimeoutSeconds">Operation timeout.</param>
public sealed record ConfigureServerSshCommand(
    int ServerId,
    bool IsEnabled,
    string SshHostname,
    int SshPort,
    string AgentHost,
    int AgentPort,
    string AgentUsername,
    string? AgentPrivateKey,
    string AgentHostKeyFingerprint,
    int ConnectTimeoutSeconds = 10,
    int OperationTimeoutSeconds = 20);
