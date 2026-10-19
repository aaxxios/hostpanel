namespace Innovayse.Hostpanel.Domain.Partners;

/// <summary>
/// Refusal codes the <see cref="Partner"/> aggregate throws as
/// <see cref="Innovayse.Hostpanel.Domain.DomainRuleException"/>.
/// </summary>
/// <remarks>
/// Declared here, not in <c>Application.Common.Errors.HostpanelErrorCodes</c>, because Domain may
/// not reference Application; <c>HostpanelErrorCodes</c> re-declares the same string values from
/// these constants so the resource coverage test still requires a sentence for each one.
/// </remarks>
public static class PartnerErrors
{
    /// <summary>The sort order is negative (400) — <see cref="Partner.Create"/>, <see cref="Partner.Update"/> and <see cref="Partner.SetSortOrder"/>.</summary>
    public const string SortOrderNegative = "PARTNER_SORT_ORDER_NEGATIVE";
}
