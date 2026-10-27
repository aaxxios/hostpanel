namespace Innovayse.Hostpanel.Domain.Ssh;

/// <summary>A HostPanel-managed key as the server reports it.</summary>
/// <param name="KeyId">The stable id written into the key's comment.</param>
/// <param name="Fingerprint">SHA256 fingerprint computed by the server.</param>
public sealed record InstalledSshKey(Guid KeyId, string Fingerprint);
