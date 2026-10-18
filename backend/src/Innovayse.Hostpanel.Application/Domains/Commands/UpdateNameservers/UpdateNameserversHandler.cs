using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Commands.UpdateNameservers;

/// <summary>
/// Replaces the nameserver list for a domain at both the registrar and aggregate levels.
/// </summary>
public sealed class UpdateNameserversHandler
{
    /// <summary>The registrar provider the new nameserver list is synced to.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>The data context the domain aggregate is loaded, mutated and saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work the aggregate is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="registrar">The registrar provider the new nameserver list is synced to.</param>
    /// <param name="context">The data context the domain aggregate is loaded, mutated and saved through.</param>
    /// <param name="uow">Unit of work the aggregate is saved through.</param>
    public UpdateNameserversHandler(IRegistrarProvider registrar, IHostpanelDbContext context, IUnitOfWork uow)
    {
        _registrar = registrar;
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles <see cref="UpdateNameserversCommand"/>.
    /// </summary>
    /// <param name="cmd">The update nameservers command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>DOMAIN_NOT_FOUND</c> when the id names no domain, or
    /// <c>NAMESERVERS_UPSTREAM_FAILED</c> when the registrar rejects the update.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateNameserversCommand cmd, CancellationToken ct)
    {
        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Id == cmd.DomainId!.Value, ct);
        if (domain is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        if (domain.RegistrarRef is not null)
        {
            var result = await _registrar.SetNameserversAsync(domain.Name, domain.RegistrarRef, cmd.Nameservers, ct);

            if (!result.Success)
            {
                return Result.Failure(
                    $"Registrar failed to update nameservers for '{domain.Name}': {result.ErrorMessage}",
                    HostpanelErrorCodes.NAMESERVERS_UPSTREAM_FAILED);
            }
        }

        domain.SetNameservers(cmd.Nameservers);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
