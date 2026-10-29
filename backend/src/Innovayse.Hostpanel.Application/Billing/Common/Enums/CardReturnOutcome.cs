namespace Innovayse.Hostpanel.Application.Billing.Common.Enums;

/// <summary>What happened to a card payment an invoice could not record.</summary>
internal enum CardReturnOutcome
{
    /// <summary>The gateway refunded it to the card.</summary>
    Refunded,

    /// <summary>The gateway refused the refund and it went to the client's credit balance.</summary>
    Credited,

    /// <summary>Neither happened, or the refund's outcome is unknown; an operator settles it.</summary>
    HeldForOperator,
}
