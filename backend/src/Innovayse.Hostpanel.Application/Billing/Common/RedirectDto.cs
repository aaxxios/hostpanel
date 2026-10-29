namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>Where the payer's browser is sent to pay on a hosted gateway page.</summary>
/// <param name="RedirectUrl">The gateway page URL.</param>
public sealed record RedirectDto(string RedirectUrl);
