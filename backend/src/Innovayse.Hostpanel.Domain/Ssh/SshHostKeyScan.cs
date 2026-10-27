namespace Innovayse.Hostpanel.Domain.Ssh;

/// <summary>The host key a server presented, observed without authenticating.</summary>
/// <param name="Algorithm">Host key algorithm, e.g. <c>ssh-ed25519</c>.</param>
/// <param name="Fingerprint">OpenSSH SHA256 fingerprint, <c>SHA256:…</c>.</param>
public sealed record SshHostKeyScan(string Algorithm, string Fingerprint);
