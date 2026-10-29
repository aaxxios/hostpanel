namespace Innovayse.Hostpanel.Shared.Settings;

/// <summary>
/// The browser origins the web edge trusts. Bound from the "Cors" section and read by both the CORS
/// policy and the payment gateway return URL check, so a payer is only ever handed back to an origin
/// the web edge already answers.
/// </summary>
public sealed class CorsOptions
{
    /// <summary>The configuration section these options bind from.</summary>
    public const string SectionName = "Cors";

    /// <summary>
    /// Exact origins (scheme and authority, e.g. <c>https://host.example.com</c>, no path), array;
    /// env form <c>Cors__AllowedOrigins__N</c>. Blank entries are allowed so a deployment can leave an
    /// optional slot unset; they are ignored. Use <see cref="Origins"/> to read the list.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];

    /// <summary>The configured origins with blank entries dropped; empty trusts no origin.</summary>
    public string[] Origins => AllowedOrigins.Where(origin => !string.IsNullOrWhiteSpace(origin)).ToArray();

    /// <summary>Whether every configured origin is an absolute URI.</summary>
    public bool IsValid => Origins.All(origin => Uri.TryCreate(origin, UriKind.Absolute, out _));
}
