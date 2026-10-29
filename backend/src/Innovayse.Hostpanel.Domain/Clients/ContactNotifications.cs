namespace Innovayse.Hostpanel.Domain.Clients;

/// <summary>Which kinds of email a <see cref="Contact"/> receives.</summary>
/// <param name="General">General account emails.</param>
/// <param name="Invoice">Invoice and billing emails.</param>
/// <param name="Support">Support ticket emails.</param>
/// <param name="Product">Product lifecycle emails.</param>
/// <param name="Domain">Domain registration emails.</param>
/// <param name="Affiliate">Affiliate program emails.</param>
public sealed record ContactNotifications(
    bool General,
    bool Invoice,
    bool Support,
    bool Product,
    bool Domain,
    bool Affiliate);
