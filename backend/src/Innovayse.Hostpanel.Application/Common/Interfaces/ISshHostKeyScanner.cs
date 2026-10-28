using Innovayse.Hostpanel.Domain.Ssh;

namespace Innovayse.Hostpanel.Application.Common.Interfaces;

/// <summary>
/// Reads the host key a server presents, without authenticating and without trusting it. Used only
/// to show the administrator what to compare against the installer's output before pinning.
/// </summary>
public interface ISshHostKeyScanner
{
    /// <summary>Connects far enough to receive the host key, then aborts.</summary>
    /// <param name="host">Host.</param>
    /// <param name="port">SSH port.</param>
    /// <param name="timeout">Connect timeout.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The presented key, or how the endpoint failed to present one.</returns>
    public Task<SshHostKeyProbe> ScanAsync(string host, int port, TimeSpan timeout, CancellationToken ct);
}
