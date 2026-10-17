using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Queries.GetDomainContact;

/// <summary>
/// Reads the registrant contact the registrar currently holds for a domain, so a form that edits it
/// starts from the real record rather than from a guess.
/// </summary>
public sealed class GetDomainContactHandler
{
    /// <summary>The data context the domain aggregate is read through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The registrar provider the contact is read from.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context the domain aggregate is read through.</param>
    /// <param name="registrar">The registrar provider the contact is read from.</param>
    public GetDomainContactHandler(IHostpanelDbContext context, IRegistrarProvider registrar)
    {
        _context = context;
        _registrar = registrar;
    }

    /// <summary>Handles <see cref="GetDomainContactQuery"/>.</summary>
    /// <param name="query">The get domain contact query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The registrant contact, or <c>DOMAIN_NOT_FOUND</c> when the id names no domain, or
    /// <c>DOMAIN_CONTACT_NOT_FOUND</c> when the registrar holds no contact record for the name.
    /// </returns>
    public async Task<Result<DomainContactDto>> HandleAsync(GetDomainContactQuery query, CancellationToken ct)
    {
        var domain = await _context.Domains.FirstOrDefaultAsync(x => x.Id == query.DomainId, ct);
        if (domain is null)
        {
            return Result.Failure<DomainContactDto>(
                $"Domain {query.DomainId} not found.", HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        var contact = await _registrar.GetContactDetailsAsync(domain.Name, ct);
        if (contact is null)
        {
            return Result.Failure<DomainContactDto>(
                $"Registrar holds no contact record for domain '{domain.Name}'.",
                HostpanelErrorCodes.DOMAIN_CONTACT_NOT_FOUND);
        }

        return Result.Success(new DomainContactDto(
            contact.FirstName, contact.LastName, contact.Organization, contact.Email, contact.Phone,
            contact.Address1, contact.Address2, contact.City, contact.State, contact.PostalCode,
            contact.Country));
    }
}
