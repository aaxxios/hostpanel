using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Registrar;

/// <summary>
/// Application service that resolves the correct <see cref="IRegistrarProvider"/>
/// for a domain name by looking up its TLD in the TLD configuration table.
/// Uses longest-suffix matching and falls back to the default provider when no configuration exists.
/// </summary>
public sealed class RegistrarProviderResolver
{
    /// <summary>The module the injected default provider belongs to; it matches the registrar the container binds as the default.</summary>
    private const RegistrarModule DefaultModule = RegistrarModule.NameAm;

    /// <summary>Database context used to read the TLD configuration.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Builds the provider named by a TLD configuration.</summary>
    private readonly IRegistrarProviderFactory _factory;

    /// <summary>Provider used when no configuration matches.</summary>
    private readonly IRegistrarProvider _defaultProvider;

    /// <summary>Initializes a new instance of the <see cref="RegistrarProviderResolver"/> class.</summary>
    /// <param name="context">Database context used to read the TLD configuration.</param>
    /// <param name="factory">Builds the provider named by a TLD configuration.</param>
    /// <param name="defaultProvider">Provider used when no configuration matches.</param>
    public RegistrarProviderResolver(
        IHostpanelDbContext context,
        IRegistrarProviderFactory factory,
        IRegistrarProvider defaultProvider)
    {
        _context = context;
        _factory = factory;
        _defaultProvider = defaultProvider;
    }

    /// <summary>
    /// Resolves the registrar provider for a domain that already exists. The registrar the domain was
    /// placed at (<see cref="RegisteredDomain.RegistrarModule"/>) wins, so a domain keeps going to its
    /// registrar after its TLD is configured for another one; a domain without a stored module (a
    /// row that predates the column, or an import whose registrar is unknown) follows its TLD.
    /// </summary>
    /// <param name="domain">The stored domain to resolve a provider for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The <see cref="IRegistrarProvider"/> that holds the domain.</returns>
    public async Task<IRegistrarProvider> ResolveAsync(RegisteredDomain domain, CancellationToken ct)
    {
        if (domain.RegistrarModule is { } module)
        {
            return _factory.GetProvider(module);
        }

        return (await ResolveForTldAsync(domain.Name, ct)).Provider;
    }

    /// <summary>
    /// Resolves the registrar for a domain name by looking up its TLD in the TldConfig table.
    /// Uses longest-suffix matching (e.g. "co.am" before "am").
    /// Falls back to the default provider if no TldConfig exists. Used for a name that has no stored
    /// domain yet (availability, WHOIS, a new registration or transfer) and by
    /// <see cref="ResolveAsync(RegisteredDomain, CancellationToken)"/> for a domain with no stored module.
    /// </summary>
    /// <param name="domainName">The fully qualified domain name to resolve a registrar for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The provider responsible for the name's TLD and the module that names it.</returns>
    public async Task<ResolvedRegistrar> ResolveForTldAsync(string domainName, CancellationToken ct)
    {
        var candidates = ExtractTldCandidates(domainName);

        foreach (var tld in candidates)
        {
            var normalizedTld = tld.ToLower().Trim();
            var config = await _context.TldConfigs.FirstOrDefaultAsync(x => x.Tld == normalizedTld, ct);
            if (config is not null)
            {
                return new ResolvedRegistrar(_factory.GetProvider(config.RegistrarModule), config.RegistrarModule);
            }
        }

        return new ResolvedRegistrar(_defaultProvider, DefaultModule);
    }

    /// <summary>
    /// Extracts all possible TLD suffixes from a domain name, ordered longest first.
    /// For "example.co.am" returns ["co.am", "am"].
    /// For "test.example.com" returns ["example.com", "com"].
    /// </summary>
    /// <param name="domainName">The fully qualified domain name to extract TLD candidates from.</param>
    /// <returns>A list of TLD suffixes ordered by length descending.</returns>
    internal static List<string> ExtractTldCandidates(string domainName)
    {
        var parts = domainName.ToLowerInvariant().Trim().Split('.');
        var candidates = new List<string>();

        // Start from index 1 (skip the SLD), generate suffixes longest first
        for (var i = 1; i < parts.Length; i++)
        {
            candidates.Add(string.Join('.', parts[i..]));
        }

        return candidates;
    }
}
