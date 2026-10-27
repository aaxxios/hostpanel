namespace Innovayse.Hostpanel.Domain.Ssh;

/// <summary>A newly generated ED25519 management identity.</summary>
/// <param name="PrivateKey">OpenSSH-format private key. Stored encrypted, never returned by the API.</param>
/// <param name="PublicKey">The <c>ssh-ed25519 …</c> public key line handed to the installer.</param>
public sealed record SshIdentity(string PrivateKey, string PublicKey);
