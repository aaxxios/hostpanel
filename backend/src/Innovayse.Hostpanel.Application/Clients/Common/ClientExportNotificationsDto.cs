namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>
/// Which kinds of email an account or one of its contacts wants, as a client data export lists them.
/// </summary>
/// <param name="General">General account emails.</param>
/// <param name="Invoice">Invoice and billing emails.</param>
/// <param name="Support">Support ticket emails.</param>
/// <param name="Product">Product and service lifecycle emails.</param>
/// <param name="Domain">Domain registration, renewal and expiry emails.</param>
/// <param name="Affiliate">Affiliate program emails.</param>
public sealed record ClientExportNotificationsDto(
    bool General,
    bool Invoice,
    bool Support,
    bool Product,
    bool Domain,
    bool Affiliate);
