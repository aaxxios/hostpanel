using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Commands.CancelTransfer;

/// <summary>
/// Cancels a pending incoming domain transfer at the registrar and marks the domain aggregate as cancelled.
/// </summary>
public sealed class CancelTransferHandler
{
    /// <summary>The registrar provider the cancellation is placed against.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>The data context the domain aggregate is loaded, mutated and saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work the aggregate is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="registrar">The registrar provider the cancellation is placed against.</param>
    /// <param name="context">The data context the domain aggregate is loaded, mutated and saved through.</param>
    /// <param name="uow">Unit of work the aggregate is saved through.</param>
    public CancelTransferHandler(IRegistrarProvider registrar, IHostpanelDbContext context, IUnitOfWork uow)
    {
        _registrar = registrar;
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles <see cref="CancelTransferCommand"/>.
    /// </summary>
    /// <param name="cmd">The cancel transfer command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>DOMAIN_NOT_FOUND</c> when the id names no domain, or
    /// <c>DOMAIN_TRANSFER_CANCEL_UPSTREAM_FAILED</c> when the registrar rejects the cancellation.
    /// </returns>
    public async Task<Result> HandleAsync(CancelTransferCommand cmd, CancellationToken ct)
    {
        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Id == cmd.DomainId, ct);
        if (domain is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        var result = await _registrar.CancelTransferAsync(domain.Name, domain.RegistrarRef ?? string.Empty, ct);

        if (!result.Success)
        {
            return Result.Failure(
                $"Registrar rejected transfer cancellation for '{domain.Name}': {result.ErrorMessage}",
                HostpanelErrorCodes.DOMAIN_TRANSFER_CANCEL_UPSTREAM_FAILED);
        }

        domain.Cancel();
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
