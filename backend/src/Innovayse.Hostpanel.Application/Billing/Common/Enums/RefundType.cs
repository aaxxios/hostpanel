namespace Innovayse.Hostpanel.Application.Billing.Common.Enums;

/// <summary>How a refund against a paid invoice is processed. Travels on the wire by name.</summary>
public enum RefundType
{
    /// <summary>The refund is executed through the hosted-gateway payment plugin.</summary>
    Gateway = 0,

    /// <summary>The refund was made outside the system and is only recorded here.</summary>
    Manual = 1,

    /// <summary>The refund amount is added to the client's credit balance.</summary>
    CreditBalance = 2,
}
