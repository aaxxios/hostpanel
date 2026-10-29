namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// The result of submitting a code. <see cref="BackupCodes"/> is present when this
/// completed an enrolment and null when it satisfied a re-auth challenge instead.
/// </summary>
/// <param name="BackupCodes">Recovery codes when the code completed an enrolment; null otherwise.</param>
public sealed record SsoTotpVerification(IReadOnlyList<string>? BackupCodes);
