namespace Innovayse.Hostpanel.Shared.Constants;

/// <summary>Locale defaults that are not a language: what a request without a usable header gets.</summary>
public static class LocaleConstants
{
    /// <summary>The IANA time zone used when <c>X-Timezone</c> is missing or not a known zone.</summary>
    public const string DefaultTimezone = "UTC";
}
