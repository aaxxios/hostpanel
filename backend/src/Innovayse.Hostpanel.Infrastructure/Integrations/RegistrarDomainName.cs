namespace Innovayse.Hostpanel.Infrastructure.Integrations;

/// <summary>
/// A fully-qualified domain name split the way registrar APIs address it: the first label and
/// the remaining suffix.
/// </summary>
/// <param name="Label">The first label (e.g. <c>example</c> of <c>example.co.uk</c>).</param>
/// <param name="Tld">The remainder after the first label (e.g. <c>co.uk</c>).</param>
internal sealed record RegistrarDomainName(string Label, string Tld)
{
    /// <summary>Splits a fully-qualified domain name into its first label and the remainder.</summary>
    /// <param name="domainName">Fully-qualified domain name (e.g. <c>example.co.uk</c>).</param>
    /// <returns>The split name.</returns>
    public static RegistrarDomainName Split(string domainName)
    {
        var parts = domainName.Split('.');
        return new RegistrarDomainName(parts[0], string.Join(".", parts[1..]));
    }
}
