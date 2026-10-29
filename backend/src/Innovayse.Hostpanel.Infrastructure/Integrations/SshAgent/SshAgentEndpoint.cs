namespace Innovayse.Hostpanel.Infrastructure.Integrations.SshAgent;

/// <summary>Where and how to reach one server's key agent. Built from that server's settings row only.</summary>
/// <param name="ServerId">The server, for logs.</param>
/// <param name="Host">Agent host.</param>
/// <param name="Port">Agent SSH port.</param>
/// <param name="Username">Management user.</param>
/// <param name="PrivateKey">OpenSSH private key text.</param>
/// <param name="HostKeyFingerprint">Pinned host key fingerprint, <c>SHA256:</c> prefixed.</param>
/// <param name="ConnectTimeout">Connect timeout.</param>
/// <param name="OperationTimeout">Per-operation timeout.</param>
public sealed record SshAgentEndpoint(
    int ServerId,
    string Host,
    int Port,
    string Username,
    string PrivateKey,
    string HostKeyFingerprint,
    TimeSpan ConnectTimeout,
    TimeSpan OperationTimeout);
