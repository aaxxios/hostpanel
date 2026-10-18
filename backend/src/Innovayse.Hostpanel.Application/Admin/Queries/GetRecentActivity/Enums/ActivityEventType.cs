namespace Innovayse.Hostpanel.Application.Admin.Queries.GetRecentActivity.Enums;

/// <summary>The kinds of event the admin notification feed surfaces.</summary>
public enum ActivityEventType
{
    /// <summary>A new client account was created.</summary>
    ClientRegistered = 0,

    /// <summary>An invoice's due date has passed without payment.</summary>
    InvoiceOverdue = 1,

    /// <summary>A domain is approaching its expiry date.</summary>
    DomainExpiring = 2,
}
