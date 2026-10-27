namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>The billing engine's settings, parsed.</summary>
/// <param name="InvoiceDaysBefore">Lead time for renewal invoices, in days.</param>
/// <param name="ReminderDays">Reminder thresholds, largest first; empty means none.</param>
/// <param name="AutoApplyCredit">Whether credit pays renewal invoices automatically.</param>
/// <param name="SuspendAfterOverdueDays">Grace before suspending overdue services, in days.</param>
public sealed record RenewalSettings(
    int InvoiceDaysBefore, IReadOnlyList<int> ReminderDays, bool AutoApplyCredit, int SuspendAfterOverdueDays)
{
    /// <inheritdoc/>
    public bool Equals(RenewalSettings? other) =>
        other is not null
        && InvoiceDaysBefore == other.InvoiceDaysBefore
        && ReminderDays.SequenceEqual(other.ReminderDays)
        && AutoApplyCredit == other.AutoApplyCredit
        && SuspendAfterOverdueDays == other.SuspendAfterOverdueDays;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(InvoiceDaysBefore, AutoApplyCredit, SuspendAfterOverdueDays);
}
