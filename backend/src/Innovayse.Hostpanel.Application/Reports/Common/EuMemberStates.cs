namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>
/// The member states of the European Union, by ISO 3166-1 alpha-2 code as client records store
/// them (Greece is <c>GR</c> here, though VAT returns write <c>EL</c>). The VAT MOSS settlement
/// covers only these countries, so the report lists nothing else.
/// </summary>
public static class EuMemberStates
{
    /// <summary>The 27 member-state codes, in alphabetical order.</summary>
    public static readonly string[] Codes =
    [
        "AT", "BE", "BG", "CY", "CZ", "DE", "DK", "EE", "ES", "FI", "FR", "GR", "HR", "HU",
        "IE", "IT", "LT", "LU", "LV", "MT", "NL", "PL", "PT", "RO", "SE", "SI", "SK",
    ];
}
