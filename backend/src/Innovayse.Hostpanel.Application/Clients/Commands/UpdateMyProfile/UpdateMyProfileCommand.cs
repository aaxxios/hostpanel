namespace Innovayse.Hostpanel.Application.Clients.Commands.UpdateMyProfile;

/// <summary>
/// Command for the authenticated client to update their own profile and billing address.
/// </summary>
/// <remarks>
/// Carries no client id and no currency. Which account is resolved inside the handler from the
/// credential, and a client's currency is set once, at their first order or by the admin who
/// created them -- the portal form still sends the field, and it is not read.
/// </remarks>
/// <param name="Email">Updated email address (null to keep current).</param>
/// <param name="FirstName">Updated first name.</param>
/// <param name="LastName">Updated last name.</param>
/// <param name="CompanyName">Updated company name (null to clear).</param>
/// <param name="Phone">Updated phone number (null to clear).</param>
/// <param name="Street">Updated billing street.</param>
/// <param name="Address2">Updated second address line.</param>
/// <param name="City">Updated billing city.</param>
/// <param name="State">Updated billing state.</param>
/// <param name="PostCode">Updated billing postcode.</param>
/// <param name="Country">Updated billing country code (2 chars, ISO 3166-1 alpha-2).</param>
/// <param name="Language">Preferred UI language code, or null to leave it alone.</param>
/// <param name="PaymentMethod">Payment method label (null to keep current).</param>
/// <param name="BillingContact">Billing contact reference (null to keep current).</param>
/// <param name="AdminNotes">Internal admin notes (null to keep current).</param>
/// <param name="NotifyGeneral">General email notifications.</param>
/// <param name="NotifyInvoice">Invoice email notifications.</param>
/// <param name="NotifySupport">Support email notifications.</param>
/// <param name="NotifyProduct">Product email notifications.</param>
/// <param name="NotifyDomain">Domain email notifications.</param>
/// <param name="NotifyAffiliate">Affiliate email notifications.</param>
/// <param name="LateFees">Apply late fees.</param>
/// <param name="OverdueNotices">Send overdue notices.</param>
/// <param name="TaxExempt">Tax exempt flag.</param>
/// <param name="SeparateInvoices">Separate invoices per service.</param>
/// <param name="DisableCcProcessing">Disable CC auto-charge.</param>
/// <param name="MarketingOptIn">Marketing emails opt-in.</param>
/// <param name="StatusUpdate">Send status update emails.</param>
/// <param name="AllowSso">Allow single sign-on.</param>
/// <param name="Status">Client status. Null to keep current.</param>
public sealed record UpdateMyProfileCommand(
    string? Email,
    string FirstName,
    string LastName,
    string? CompanyName,
    string? Phone,
    string? Street,
    string? Address2,
    string? City,
    string? State,
    string? PostCode,
    string? Country,
    string? Language,
    string? PaymentMethod,
    string? BillingContact,
    string? AdminNotes,
    bool NotifyGeneral,
    bool NotifyInvoice,
    bool NotifySupport,
    bool NotifyProduct,
    bool NotifyDomain,
    bool NotifyAffiliate,
    bool LateFees,
    bool OverdueNotices,
    bool TaxExempt,
    bool SeparateInvoices,
    bool DisableCcProcessing,
    bool MarketingOptIn,
    bool StatusUpdate,
    bool AllowSso,
    string? Status);
