namespace Innovayse.Hostpanel.Domain.Billing.Enums;
/// <summary>Time unit for recurring billable item intervals.</summary>
public enum RecurrencePeriod
{
    /// <summary>Recur every N days.</summary>
    Days = 0,

    /// <summary>Recur every N weeks.</summary>
    Weeks = 1,

    /// <summary>Recur every N months.</summary>
    Months = 2,

    /// <summary>Recur every N years.</summary>
    Years = 3,
}
