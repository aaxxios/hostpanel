using System.Globalization;

namespace Innovayse.Hostpanel.Infrastructure.Integrations;

/// <summary>
/// Reads the dates a registrar answers with. The one rule for it: the invariant culture, so the
/// request's culture (ru, hy) never decides how a date is read, and UTC for a date that names no
/// offset, so the same text is the same moment on every host.
/// </summary>
internal static class RegistrarDate
{
    /// <summary>Reads a registrar date.</summary>
    /// <param name="raw">The text the registrar sent; may be absent or malformed.</param>
    /// <returns>The moment, or <see langword="null"/> when the text is absent or not a date.</returns>
    public static DateTimeOffset? Parse(string? raw)
    {
        return DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed
            : null;
    }
}
