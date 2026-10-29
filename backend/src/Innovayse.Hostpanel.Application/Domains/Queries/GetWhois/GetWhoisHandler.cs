using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Registrar;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Domains.Queries.GetWhois;

/// <summary>Performs a WHOIS lookup via the registrar provider and returns a <see cref="WhoisDto"/>.</summary>
public sealed class GetWhoisHandler
{
    /// <summary>Resolves the registrar that holds the domain's TLD.</summary>
    private readonly RegistrarProviderResolver _registrars;

    /// <summary>Initializes the handler.</summary>
    /// <param name="registrars">Resolves the registrar that holds the domain's TLD.</param>
    public GetWhoisHandler(RegistrarProviderResolver registrars)
    {
        _registrars = registrars;
    }

    /// <summary>
    /// Handles <see cref="GetWhoisQuery"/>.
    /// </summary>
    /// <param name="query">The get WHOIS query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// WHOIS information for the domain, or <c>DOMAIN_WHOIS_NOT_FOUND</c> when the registrar
    /// returns no record for the name.
    /// </returns>
    public async Task<Result<WhoisDto>> HandleAsync(GetWhoisQuery query, CancellationToken ct)
    {
        var registrar = await _registrars.ResolveForTldAsync(query.DomainName, ct);
        var whois = await registrar.Provider.GetWhoisAsync(query.DomainName, ct);
        if (whois is null)
        {
            return Result.Failure<WhoisDto>(
                $"WHOIS data not found for domain '{query.DomainName}'.",
                HostpanelErrorCodes.DOMAIN_WHOIS_NOT_FOUND);
        }

        var dto = new WhoisDto(whois.Registrar, whois.Registrant, whois.CreatedAt, whois.ExpiresAt);
        return Result.Success(dto);
    }
}
