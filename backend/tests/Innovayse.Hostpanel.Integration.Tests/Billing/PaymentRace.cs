namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>Names the suspension during whose save <see cref="PayingSaveInterceptor"/> pays another invoice.</summary>
internal sealed class PaymentRace
{
    /// <summary>The service whose suspension save triggers the payment.</summary>
    public int TriggerServiceId { get; set; }

    /// <summary>The invoice marked paid when the trigger fires.</summary>
    public int InvoiceToPay { get; set; }

    /// <summary>Whether the payment was made.</summary>
    public bool Fired { get; set; }
}
