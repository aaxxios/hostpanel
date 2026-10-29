namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>The account switches staff set on a client: billing behaviour, marketing consent and sign-in options.</summary>
/// <param name="OverdueNotices">Payment reminder emails for unpaid invoices are sent.</param>
/// <param name="TaxExempt">The client is exempt from tax: no tax rate can be set on their invoices.</param>
/// <param name="SeparateInvoices">Renewals are invoiced one invoice per line instead of one per run.</param>
/// <param name="DisableCcProcessing">Card payments and saved card charges are refused for the client.</param>
/// <param name="MarketingOptIn">The client has opted in to marketing emails.</param>
/// <param name="StatusUpdate">Status update emails are sent.</param>
/// <param name="AllowSso">The client may sign in to the hosting panels of their services through single sign-on.</param>
public sealed record ClientSettingsDto(
    bool OverdueNotices,
    bool TaxExempt,
    bool SeparateInvoices,
    bool DisableCcProcessing,
    bool MarketingOptIn,
    bool StatusUpdate,
    bool AllowSso);
