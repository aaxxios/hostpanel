namespace Innovayse.Hostpanel.Domain.Locations;

/// <summary>
/// Refusal codes the <see cref="ServerLocation"/> aggregate throws as
/// <see cref="Innovayse.Hostpanel.Domain.DomainRuleException"/>.
/// </summary>
/// <remarks>
/// Declared here, not in <c>Application.Common.Errors.HostpanelErrorCodes</c>, because Domain may
/// not reference Application; <c>HostpanelErrorCodes</c> re-declares the same string values from
/// these constants so the resource coverage test still requires a sentence for each one.
/// </remarks>
public static class ServerLocationErrors
{
    /// <summary>The sort order is negative (400) — <see cref="ServerLocation.Create"/>, <see cref="ServerLocation.Update"/> and <see cref="ServerLocation.SetSortOrder"/>.</summary>
    public const string SortOrderNegative = "LOCATION_SORT_ORDER_NEGATIVE";

    /// <summary>The country code is not two letters (400) — <see cref="ServerLocation.Create"/> and <see cref="ServerLocation.Update"/>.</summary>
    public const string CountryCodeInvalid = "LOCATION_COUNTRY_CODE_INVALID";
}
