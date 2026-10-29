namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>The email preferences on the client's own profile.</summary>
/// <param name="General">General account emails enabled.</param>
/// <param name="Invoice">Invoice emails enabled.</param>
/// <param name="Support">Support emails enabled.</param>
/// <param name="Product">Product emails enabled.</param>
/// <param name="Domain">Domain emails enabled.</param>
/// <param name="Affiliate">Affiliate emails enabled.</param>
/// <param name="MarketingOptIn">Marketing emails opt-in.</param>
/// <param name="StatusUpdate">Status update emails sent.</param>
public sealed record MyProfileNotificationsDto(
    bool General,
    bool Invoice,
    bool Support,
    bool Product,
    bool Domain,
    bool Affiliate,
    bool MarketingOptIn,
    bool StatusUpdate);
