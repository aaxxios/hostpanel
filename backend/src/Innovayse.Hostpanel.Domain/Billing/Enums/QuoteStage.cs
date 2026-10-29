namespace Innovayse.Hostpanel.Domain.Billing.Enums;

/// <summary>Lifecycle stages for a <see cref="Quote"/>.</summary>
public enum QuoteStage
{
    /// <summary>Quote is being prepared and has not been sent to the client.</summary>
    Draft = 0,

    /// <summary>Quote has been delivered to the client for review.</summary>
    Delivered = 1,

    /// <summary>Quote is temporarily paused pending further discussion.</summary>
    OnHold = 2,

    /// <summary>Quote has been accepted by the client.</summary>
    Accepted = 3,

    /// <summary>Quote was not accepted and the opportunity is lost.</summary>
    Lost = 4,

    /// <summary>Quote expiry date has passed and it is no longer valid.</summary>
    Expired = 5,

    /// <summary>Quote has been permanently abandoned.</summary>
    Dead = 6,
}
