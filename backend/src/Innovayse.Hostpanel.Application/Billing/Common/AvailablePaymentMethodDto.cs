namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>
/// One payment method offered at checkout.
/// </summary>
/// <param name="Module">Module id the checkout sends back as the order's payment method.</param>
/// <param name="DisplayName">Label the payer sees; serialized as <c>displayName</c>.</param>
public sealed record AvailablePaymentMethodDto(string Module, string DisplayName);
