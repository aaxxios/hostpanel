namespace Innovayse.Hostpanel.Domain.Billing;

/// <summary>
/// The recurring-period spellings a billable item stores, compared case-insensitively.
/// </summary>
public static class RecurringPeriodNames
{
    /// <summary>Every month.</summary>
    public const string Monthly = "monthly";

    /// <summary>Every three months.</summary>
    public const string Quarterly = "quarterly";

    /// <summary>Every year.</summary>
    public const string Annually = "annually";

    /// <summary>Every year, the alternative spelling.</summary>
    public const string Yearly = "yearly";

    /// <summary>Every week.</summary>
    public const string Weekly = "weekly";
}
