using System.Text.Json.Serialization;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>Wire shape of the sign-on provider answer to a submitted TOTP code.</summary>
/// <param name="BackupCodes">Recovery codes when the code completed an enrolment; null otherwise.</param>
internal sealed record SsoTotpVerificationWire(
    [property: JsonPropertyName("backupCodes")] IReadOnlyList<string>? BackupCodes);
