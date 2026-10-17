namespace Innovayse.Hostpanel.Domain.Billing.Enums;
/// <summary>Lifecycle states for a <see cref="Quote"/>.</summary>
public enum QuoteStatus
{
    /// <summary>Quote is saved but not yet sent to the client.</summary>
    Draft = 0,

    /// <summary>Quote has been sent to the client awaiting response.</summary>
    Sent = 1,

    /// <summary>Client has accepted the quote and it can be converted to an invoice.</summary>
    Accepted = 2,

    /// <summary>Client has declined the quote.</summary>
    Declined = 3,

    /// <summary>Quote expiry date has passed and it is no longer valid.</summary>
    Expired = 4,

    /// <summary>Quote has been cancelled and will not be pursued.</summary>
    Cancelled = 5,
}
