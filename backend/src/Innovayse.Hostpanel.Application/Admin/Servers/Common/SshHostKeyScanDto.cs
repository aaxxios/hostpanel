namespace Innovayse.Hostpanel.Application.Admin.Servers.Common;

/// <summary>The host key a server presented to a scan, for the administrator to verify.</summary>
/// <param name="Algorithm">Host key algorithm.</param>
/// <param name="Fingerprint">SHA256 fingerprint.</param>
/// <param name="MatchesPinned">Whether it equals the fingerprint already pinned.</param>
public sealed record SshHostKeyScanDto(string Algorithm, string Fingerprint, bool MatchesPinned);
