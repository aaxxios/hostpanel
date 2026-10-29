namespace Innovayse.Hostpanel.Application.Billing.Common.Enums;

/// <summary>
/// The kind of a payment method the card gateway keeps on file for a customer. Travels on the wire by name;
/// each gateway adapter maps its own type codes to it.
/// </summary>
public enum SavedPaymentMethodType
{
    /// <summary>A debit or credit card.</summary>
    Card = 0,

    /// <summary>A bank account debited directly.</summary>
    BankAccount = 1,

    /// <summary>Any other method the gateway reports that has no dedicated kind here.</summary>
    Other = 2,
}
