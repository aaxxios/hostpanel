using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Commands.ModifyDomainContact;

/// <summary>
/// Sends updated WHOIS registrant contact details to the registrar for a domain.
/// </summary>
public sealed class ModifyDomainContactHandler
{
    /// <summary>The data context the domain aggregate is read through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The registrar provider the contact update is sent to.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context the domain aggregate is read through.</param>
    /// <param name="registrar">The registrar provider the contact update is sent to.</param>
    public ModifyDomainContactHandler(IHostpanelDbContext context, IRegistrarProvider registrar)
    {
        _context = context;
        _registrar = registrar;
    }

    /// <summary>
    /// Handles <see cref="ModifyDomainContactCommand"/>.
    /// </summary>
    /// <param name="cmd">The modify domain contact command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>DOMAIN_NOT_FOUND</c> when the id names no domain, or
    /// <c>DOMAIN_CONTACT_UPSTREAM_FAILED</c> when the registrar rejects the contact update.
    /// </returns>
    public async Task<Result> HandleAsync(ModifyDomainContactCommand cmd, CancellationToken ct)
    {
        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Id == cmd.DomainId!.Value, ct);
        if (domain is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        var contact = new DomainContact(
            cmd.FirstName, cmd.LastName, cmd.Organization, cmd.Email, cmd.Phone,
            cmd.Address1, cmd.Address2, cmd.City, cmd.State, cmd.PostalCode, cmd.Country);
        var result = await _registrar.ModifyContactDetailsAsync(domain.Name, contact, ct);
        if (!result.Success)
        {
            return Result.Failure(
                $"Registrar failed to modify contact details for '{domain.Name}': {result.ErrorMessage}",
                HostpanelErrorCodes.DOMAIN_CONTACT_UPSTREAM_FAILED);
        }

        return Result.Success();
    }
}
