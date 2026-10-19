using Innovayse.Hostpanel.Domain.Clients;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// The host names the demo customers' services and domains sit under, derived from the company name so
/// every seeder that needs "this customer's site" agrees on it without sharing state.
/// </summary>
internal static class DemoSites
{
    /// <summary>The suffix of every customer's main site: a name reserved by RFC 2606, so no seeded host can resolve to a real site.</summary>
    internal const string Suffix = ".example";

    /// <summary>The lower-case letters of the first word of the company (or first name) name.</summary>
    /// <param name="client">The customer.</param>
    /// <returns>A short label such as <c>brightleaf</c>.</returns>
    internal static string LabelOf(Client client)
    {
        var name = client.CompanyName ?? client.FirstName;
        var firstWord = name.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
        return new string(firstWord.Where(char.IsLetter).ToArray()).ToLowerInvariant();
    }

    /// <summary>The customer's main site.</summary>
    /// <param name="client">The customer.</param>
    /// <returns>The label plus <see cref="Suffix"/>.</returns>
    internal static string HostOf(Client client) => LabelOf(client) + Suffix;
}
