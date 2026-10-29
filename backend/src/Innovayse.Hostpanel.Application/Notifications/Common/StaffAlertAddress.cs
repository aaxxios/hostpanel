using Innovayse.Hostpanel.Application.Common.Settings;

namespace Innovayse.Hostpanel.Application.Notifications.Common;

/// <summary>
/// Picks the mailbox a staff alert goes to. A deliberate exception to <see cref="NotificationOptions"/>'
/// "no fallback" rule: <see cref="NotificationOptions.AdminEmail"/> is commonly unset, and these
/// alerts are about money or hosting accounts that need a person, so they must reach some mailbox a
/// person reads rather than nobody.
/// </summary>
public static class StaffAlertAddress
{
    /// <summary>Finds the address a staff alert is mailed to.</summary>
    /// <param name="options">The notification options.</param>
    /// <returns>The admin address, else the contact address, or <see langword="null"/> when neither is set.</returns>
    public static string? Resolve(NotificationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!string.IsNullOrWhiteSpace(options.AdminEmail))
        {
            return options.AdminEmail;
        }

        return string.IsNullOrWhiteSpace(options.ContactEmail) ? null : options.ContactEmail;
    }
}
