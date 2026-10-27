using System.Text.RegularExpressions;

namespace Innovayse.Hostpanel.Domain.Settings;

/// <summary>
/// The billing engine's operator settings (issue #67). Kept apart from <see cref="PortalSettingKeys"/>
/// on purpose: that list feeds the anonymous storefront endpoint, and none of these is public.
/// Seeded by <c>PortalSettingsSeeder</c>, edited in System Settings, validated by <c>UpdateSettingHandler</c>.
/// </summary>
public static class BillingSettingKeys
{
    /// <summary>Days before the renewal date a renewal invoice is raised.</summary>
    public const string InvoiceDaysBefore = "billing.renewal.invoice_days_before";

    /// <summary>Comma-separated days before due on which a reminder is mailed; empty turns reminders off.</summary>
    public const string ReminderDays = "billing.renewal.reminder_days";

    /// <summary>Whether a client's credit balance pays renewal invoices automatically.</summary>
    public const string AutoApplyCredit = "billing.renewal.auto_apply_credit";

    /// <summary>Days an invoice may stay overdue before the services on it are suspended.</summary>
    public const string SuspendAfterOverdueDays = "billing.renewal.suspend_after_overdue_days";

    /// <summary>
    /// The largest day count any of these settings accepts (the day-count and reminder patterns
    /// stop at 90). A reader that must look as far ahead as any setting can reach uses this instead
    /// of its own literal; the renewal cron does not, it reads the configured lead time.
    /// </summary>
    public const int MaxDays = 90;

    // Declared before Patterns: static fields initialise in textual order.
    // At least one day: a zero lead time would raise the invoice on the renewal date itself, due
    // that same day, leaving the client no time to pay before the period starts.
    private static readonly ValuePattern LeadDays = new(
        new Regex("^([1-9]|[1-8][0-9]|90)$", RegexOptions.CultureInvariant),
        "a whole number of days from 1 to 90",
        v => v);

    private static readonly ValuePattern DayCount = new(
        new Regex("^([0-9]|[1-8][0-9]|90)$", RegexOptions.CultureInvariant),
        "a whole number of days from 0 to 90",
        v => v);

    /// <summary>The shape each key accepts, with its canonical spelling.</summary>
    public static readonly IReadOnlyDictionary<string, ValuePattern> Patterns =
        new Dictionary<string, ValuePattern>
        {
            [InvoiceDaysBefore] = LeadDays,
            [SuspendAfterOverdueDays] = DayCount,
            [ReminderDays] = new(
                new Regex("^\\s*([1-9]|[1-8][0-9]|90)\\s*(,\\s*([1-9]|[1-8][0-9]|90)\\s*)*$", RegexOptions.CultureInvariant),
                "comma-separated days from 1 to 90, like 7,3,1",
                v => string.Join(',', v.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))),
            [AutoApplyCredit] = new(
                new Regex("^(true|false)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
                "true or false",
                v => v.ToLowerInvariant()),
        };

    /// <summary>Every key with its seeded value and the description shown in System Settings.</summary>
    public static readonly IReadOnlyList<(string Key, string Value, string Description)> Defaults =
    [
        (InvoiceDaysBefore, "14", "Days before a service or domain renews that its renewal invoice is raised."),
        (ReminderDays, "7,3,1", "Days before a renewal invoice is due on which a reminder is mailed. Empty sends none."),
        (AutoApplyCredit, "true", "Pay renewal invoices from the client's credit balance automatically (true/false)."),
        (SuspendAfterOverdueDays, "3", "Days an invoice may stay overdue before the services on it are suspended."),
    ];

    /// <summary>Keys that may be saved empty although they seed with a value.</summary>
    private static readonly IReadOnlySet<string> Clearable = new HashSet<string>(StringComparer.Ordinal) { ReminderDays };

    /// <summary>Whether <paramref name="key"/> is a billing key that may be saved empty.</summary>
    /// <param name="key">The setting key.</param>
    /// <returns><see langword="true"/> for a clearable billing key.</returns>
    public static bool AllowsEmpty(string key) => Clearable.Contains(key);
}
