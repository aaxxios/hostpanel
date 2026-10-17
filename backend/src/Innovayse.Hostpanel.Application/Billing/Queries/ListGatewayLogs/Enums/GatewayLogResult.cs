namespace Innovayse.Hostpanel.Application.Billing.Queries.ListGatewayLogs.Enums;

/// <summary>How a recorded gateway interaction ended, as the admin gateway log reports it.</summary>
public enum GatewayLogResult
{
    /// <summary>A payment was recorded against an invoice.</summary>
    Payment = 0,

    /// <summary>A refund was recorded against an invoice.</summary>
    Refund = 1,

    /// <summary>A credit was applied to an invoice.</summary>
    Credit = 2,

    /// <summary>A hosted-gateway payment session was started and has not been settled yet.</summary>
    Pending = 3,
}
