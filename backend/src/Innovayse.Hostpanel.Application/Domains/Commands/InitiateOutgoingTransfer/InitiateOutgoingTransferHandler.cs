using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Commands.InitiateOutgoingTransfer;

/// <summary>
/// Initiates an outgoing domain transfer at the registrar, retrieves the EPP code,
/// and stores it on the domain aggregate.
/// </summary>
public sealed class InitiateOutgoingTransferHandler
{
    /// <summary>The registrar provider the outgoing transfer is placed against.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>The data context the domain aggregate is loaded, mutated and saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work the aggregate is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="registrar">The registrar provider the outgoing transfer is placed against.</param>
    /// <param name="context">The data context the domain aggregate is loaded, mutated and saved through.</param>
    /// <param name="uow">Unit of work the aggregate is saved through.</param>
    public InitiateOutgoingTransferHandler(IRegistrarProvider registrar, IHostpanelDbContext context, IUnitOfWork uow)
    {
        _registrar = registrar;
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles <see cref="InitiateOutgoingTransferCommand"/>.
    /// </summary>
    /// <param name="cmd">The initiate outgoing transfer command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The EPP (authorization) code to hand to the gaining registrar, or <c>DOMAIN_NOT_FOUND</c> when
    /// the id names no domain, or <c>DOMAIN_NO_REGISTRAR_REF</c> when the domain carries no registrar
    /// reference, or <c>DOMAIN_OUTGOING_TRANSFER_UPSTREAM_FAILED</c> when the registrar rejects the
    /// request, or <c>DOMAIN_EPP_CODE_UPSTREAM_FAILED</c> when the registrar returns no code.
    /// </returns>
    public async Task<Result<string>> HandleAsync(InitiateOutgoingTransferCommand cmd, CancellationToken ct)
    {
        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Id == cmd.DomainId, ct);
        if (domain is null)
        {
            return Result.Failure<string>(
                $"Domain {cmd.DomainId} not found.", HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        if (domain.RegistrarRef is null)
        {
            return Result.Failure<string>(
                $"Domain '{domain.Name}' has no registrar reference — cannot initiate outgoing transfer.",
                HostpanelErrorCodes.DOMAIN_NO_REGISTRAR_REF);
        }

        var result = await _registrar.InitiateOutgoingTransferAsync(domain.Name, domain.RegistrarRef, ct);

        if (!result.Success)
        {
            return Result.Failure<string>(
                $"Registrar rejected outgoing transfer initiation for '{domain.Name}': {result.ErrorMessage}",
                HostpanelErrorCodes.DOMAIN_OUTGOING_TRANSFER_UPSTREAM_FAILED);
        }

        var eppCode = await _registrar.GetEppCodeAsync(domain.Name, domain.RegistrarRef, ct);
        if (eppCode is null)
        {
            return Result.Failure<string>(
                $"Registrar did not return an EPP code for domain '{domain.Name}'.",
                HostpanelErrorCodes.DOMAIN_EPP_CODE_UPSTREAM_FAILED);
        }

        domain.SetEppCode(eppCode);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(eppCode);
    }
}
