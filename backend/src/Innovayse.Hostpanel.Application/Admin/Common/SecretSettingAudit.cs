using Innovayse.Hostpanel.Application.Admin.Common.Enums;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Admin.Common;

/// <summary>
/// Writes the audit line for a change to a secret setting. Only the key, the kind of change and the
/// acting user are recorded; the value is never passed in, so it cannot reach a log sink.
/// </summary>
/// <remarks>
/// The client activity log is scoped to one client account and has no place for a system setting,
/// so the structured application log is this product's audit trail for configuration changes.
/// </remarks>
public static class SecretSettingAudit
{
    /// <summary>Records that a secret setting changed.</summary>
    /// <param name="logger">The logger of the handler that made the change.</param>
    /// <param name="settingKey">The full setting key.</param>
    /// <param name="change">Whether the value was replaced or cleared.</param>
    /// <param name="userId">The acting user's id, or <c>null</c> when it is not known.</param>
    public static void Record(ILogger logger, string settingKey, SecretSettingChange change, string? userId)
    {
        ArgumentNullException.ThrowIfNull(logger);

        logger.LogInformation(
            "Secret setting {SettingKey} {SecretChange} by user {UserId}", settingKey, change, userId);
    }
}
