namespace Innovayse.Hostpanel.Domain.Billing.Enums;
/// <summary>Classification for billable items.</summary>
public enum BillableItemType
{
    /// <summary>One-time charge that is not recurring.</summary>
    OneTime = 0,

    /// <summary>Recurring charge that repeats monthly, quarterly, or annually.</summary>
    Recurring = 1,
}
