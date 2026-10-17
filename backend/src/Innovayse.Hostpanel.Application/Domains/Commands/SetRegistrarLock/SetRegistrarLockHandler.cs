using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Commands.SetRegistrarLock;

/// <summary>
/// Enables or disables the registrar transfer-lock for a domain at both the registrar and
/// aggregate levels.
/// </summary>
public sealed class SetRegistrarLockHandler
{
    /// <summary>The registrar provider the lock flag is synced to.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>The data context the domain aggregate is loaded, mutated and saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work the aggregate is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="registrar">The registrar provider the lock flag is synced to.</param>
    /// <param name="context">The data context the domain aggregate is loaded, mutated and saved through.</param>
    /// <param name="uow">Unit of work the aggregate is saved through.</param>
    public SetRegistrarLockHandler(IRegistrarProvider registrar, IHostpanelDbContext context, IUnitOfWork uow)
    {
        _registrar = registrar;
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles <see cref="SetRegistrarLockCommand"/>.
    /// </summary>
    /// <param name="cmd">The set registrar lock command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>DOMAIN_NOT_FOUND</c> when the id names no domain, or
    /// <c>REGISTRAR_LOCK_UPSTREAM_FAILED</c> when the registrar rejects the update.
    /// </returns>
    public async Task<Result> HandleAsync(SetRegistrarLockCommand cmd, CancellationToken ct)
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
            var result = await _registrar.SetRegistrarLockAsync(domain.Name, domain.RegistrarRef, cmd.Enabled, ct);

            if (!result.Success)
            {
                return Result.Failure(
                    $"Registrar failed to update lock status for '{domain.Name}': {result.ErrorMessage}",
                    HostpanelErrorCodes.REGISTRAR_LOCK_UPSTREAM_FAILED);
            }
        }

        domain.SetLock(cmd.Enabled);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
