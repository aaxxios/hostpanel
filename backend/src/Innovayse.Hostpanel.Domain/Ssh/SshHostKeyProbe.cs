using Innovayse.Hostpanel.Domain.Ssh.Enums;

namespace Innovayse.Hostpanel.Domain.Ssh;

/// <summary>What reading an agent endpoint's host key found: the key, or how it failed.</summary>
/// <param name="Key">The presented key, when one was received.</param>
/// <param name="Failure">How the read failed, when no key was received.</param>
public sealed record SshHostKeyProbe(SshHostKeyScan? Key, SshEndpointFailure? Failure);
