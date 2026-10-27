using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Commands.RenewDomain;

/// <summary>
/// Renews an existing domain registration at the registrar and updates the aggregate's expiry date.
/// </summary>
public sealed class RenewDomainHandler
{
    /// <summary>The registrar provider used to place the renewal order.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>The data context the domain aggregate is loaded, mutated and saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work the aggregate is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="registrar">The registrar provider used to place the renewal order.</param>
    /// <param name="context">The data context the domain aggregate is loaded, mutated and saved through.</param>
    /// <param name="uow">Unit of work the aggregate is saved through.</param>
    public RenewDomainHandler(IRegistrarProvider registrar, IHostpanelDbContext context, IUnitOfWork uow)
    {
        _registrar = registrar;
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles <see cref="RenewDomainCommand"/>.
    /// </summary>
    /// <param name="cmd">The renew domain command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>DOMAIN_NOT_FOUND</c> when the id names no domain, or
    /// <c>DOMAIN_NO_REGISTRAR_REF</c> when the domain was never placed at a registrar, or
    /// <c>DOMAIN_RENEWAL_UPSTREAM_FAILED</c> when the registrar rejects the renewal.
    /// </returns>
    public async Task<Result> HandleAsync(RenewDomainCommand cmd, CancellationToken ct)
    {
        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Id == cmd.DomainId!.Value, ct);
        if (domain is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        if (domain.RegistrarRef is null)
        {
            // Never placed at a registrar, so there is nothing to renew there. Extending the
            // expiry here would mark a paid domain renewed while nothing changed upstream.
            return Result.Failure(
                $"'{domain.Name}' has no registrar reference and cannot be renewed at the registrar.",
                HostpanelErrorCodes.DOMAIN_NO_REGISTRAR_REF);
        }

        var request = new DomainRenewalOrder(domain.Name, domain.RegistrarRef, cmd.Years);
        var result = await _registrar.RenewAsync(request, ct);

        if (!result.Success)
        {
            return Result.Failure(
                $"Registrar rejected renewal of '{domain.Name}': {result.ErrorMessage}",
                HostpanelErrorCodes.DOMAIN_RENEWAL_UPSTREAM_FAILED);
        }

        var newExpiresAt = result.ExpiresAt ?? domain.ExpiresAt.AddYears(cmd.Years);

        domain.Renew(newExpiresAt);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
