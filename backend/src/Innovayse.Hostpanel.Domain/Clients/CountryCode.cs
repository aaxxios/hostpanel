namespace Innovayse.Hostpanel.Domain.Clients;

/// <summary>The one place a country code is put in the shape it is stored in.</summary>
internal static class CountryCode
{
    /// <summary>Trims the code and upper-cases it, so "am" and "AM" are the same country everywhere it is read.</summary>
    /// <param name="country">The code as entered, or <see langword="null"/> when none was given.</param>
    /// <returns>The normalised code, or <see langword="null"/> when none was given.</returns>
    internal static string? Normalize(string? country) => country?.Trim().ToUpperInvariant();
}
