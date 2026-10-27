using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Domain.Ssh;

namespace Innovayse.Hostpanel.Application.Common.Interfaces;

/// <summary>
/// Resolves the SSH key capability of a server from its <see cref="Server.Module"/> and its
/// <see cref="ServerSshSettings"/>. Answers <see langword="null"/> — explicitly unsupported —
/// for any module without an implementation, so existing provisioning providers need no change.
/// </summary>
public interface ISshKeyProviderFactory
{
    /// <summary>Whether the server's module has an SSH key implementation at all.</summary>
    /// <param name="module">The server module.</param>
    /// <returns><see langword="true"/> when supported.</returns>
    bool Supports(ServerModule module);

    /// <summary>Creates a provider bound to one server.</summary>
    /// <param name="server">The server.</param>
    /// <param name="settings">Its SSH settings; must be complete.</param>
    /// <returns>The provider, or <see langword="null"/> when the module is unsupported.</returns>
    ISshKeyProvider? CreateFor(Server server, ServerSshSettings settings);
}
