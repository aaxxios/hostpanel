namespace Innovayse.Hostpanel.Domain.Ssh.Enums;

/// <summary>
/// How an agent endpoint failed to present a host key, told apart by what the network did, so an
/// administrator is pointed at the right thing to fix.
/// </summary>
public enum SshEndpointFailure
{
    /// <summary>No answer within the timeout: no network path (VPN, routing, a firewall that drops).</summary>
    NoAnswer,

    /// <summary>The connection was refused: nothing listens on the port, or a firewall rejects it.</summary>
    Refused,

    /// <summary>
    /// The connection opened and was closed before the SSH greeting: sshd turned the address away
    /// (OpenSSH 9.8+ PerSourcePenalties, MaxStartups, TCP wrappers).
    /// </summary>
    ClosedBeforeGreeting,

    /// <summary>Any other failure, such as a service on the port that does not speak SSH.</summary>
    Other,
}
