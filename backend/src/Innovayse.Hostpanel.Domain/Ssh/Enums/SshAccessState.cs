namespace Innovayse.Hostpanel.Domain.Ssh.Enums;

/// <summary>
/// Whether a service's customer can manage SSH keys right now, and if not, why. The portal
/// branches on this value; it never needs to know which server or provider was involved.
/// </summary>
public enum SshAccessState
{
    /// <summary>Keys can be listed, added and revoked.</summary>
    Available,

    /// <summary>
    /// The server answered that this hosting account cannot use SSH: no login shell, not eligible,
    /// or not an account the server knows.
    /// </summary>
    ShellDisabled,

    /// <summary>The service's server runs a control panel with no SSH key implementation.</summary>
    Unsupported,

    /// <summary>The service has no server/account yet, or SSH key management is not enabled on its server.</summary>
    NotConfigured,

    /// <summary>The service is not active (pending, suspended or terminated).</summary>
    ServiceInactive,

    /// <summary>The server's key agent did not answer.</summary>
    Unreachable,
}
