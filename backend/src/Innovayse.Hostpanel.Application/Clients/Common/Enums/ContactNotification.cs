namespace Innovayse.Hostpanel.Application.Clients.Common.Enums;

/// <summary>
/// The kinds of email an additional contact can opt in to, one per <c>Notify*</c> switch on the
/// contact. A sender names the kind of mail it is sending and the contacts who switched that kind
/// on receive a copy.
/// </summary>
public enum ContactNotification
{
    /// <summary>General account emails.</summary>
    General = 0,

    /// <summary>Invoice and billing emails.</summary>
    Invoice = 1,

    /// <summary>Support ticket emails.</summary>
    Support = 2,

    /// <summary>Product and service lifecycle emails.</summary>
    Product = 3,

    /// <summary>Domain registration, renewal and expiry emails.</summary>
    Domain = 4,

    /// <summary>Affiliate program emails.</summary>
    Affiliate = 5,
}
