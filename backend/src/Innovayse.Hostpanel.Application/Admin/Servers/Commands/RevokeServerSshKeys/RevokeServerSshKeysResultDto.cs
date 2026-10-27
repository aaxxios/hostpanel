namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.RevokeServerSshKeys;

/// <summary>Outcome of revoking a server's managed keys.</summary>
/// <param name="Revoked">Keys the agent confirmed removed.</param>
/// <param name="Failed">Keys left <c>RevokeFailed</c>, with their reason on each key.</param>
public sealed record RevokeServerSshKeysResultDto(int Revoked, int Failed);
