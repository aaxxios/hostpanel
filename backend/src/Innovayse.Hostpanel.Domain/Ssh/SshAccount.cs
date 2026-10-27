namespace Innovayse.Hostpanel.Domain.Ssh;

/// <summary>
/// The hosting account a provider call acts on. Built by HostPanel from the service record,
/// never from anything a browser sent.
/// </summary>
/// <param name="ServerId">The server the account lives on; providers refuse a mismatch.</param>
/// <param name="Username">The Linux account name on that server.</param>
public sealed record SshAccount(int ServerId, string Username);
