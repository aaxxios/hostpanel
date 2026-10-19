namespace Innovayse.Hostpanel.Domain.Faqs;

/// <summary>
/// Refusal codes the <see cref="Faq"/> aggregate throws as
/// <see cref="Innovayse.Hostpanel.Domain.DomainRuleException"/>.
/// </summary>
/// <remarks>
/// Declared here, not in <c>Application.Common.Errors.HostpanelErrorCodes</c>, because Domain may
/// not reference Application; <c>HostpanelErrorCodes</c> re-declares the same string values from
/// these constants so the resource coverage test still requires a sentence for each one.
/// </remarks>
public static class FaqErrors
{
    /// <summary>The sort order is negative (400) — <see cref="Faq.Create"/>, <see cref="Faq.Update"/> and <see cref="Faq.SetSortOrder"/>.</summary>
    public const string SortOrderNegative = "FAQ_SORT_ORDER_NEGATIVE";
}
