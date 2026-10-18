using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Commands.SetEmailForwarding;

/// <summary>
/// Enables or disables email forwarding for a domain at both the registrar and aggregate levels.
/// </summary>
public sealed class SetEmailForwardingHandler
{
    /// <summary>The registrar provider the flag is synced to.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>The data context the domain aggregate is loaded, mutated and saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work the aggregate is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="registrar">The registrar provider the flag is synced to.</param>
    /// <param name="context">The data context the domain aggregate is loaded, mutated and saved through.</param>
    /// <param name="uow">Unit of work the aggregate is saved through.</param>
    public SetEmailForwardingHandler(IRegistrarProvider registrar, IHostpanelDbContext context, IUnitOfWork uow)
    {
        _registrar = registrar;
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles <see cref="SetEmailForwardingCommand"/>.
    /// </summary>
    /// <param name="cmd">The set email forwarding command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>DOMAIN_NOT_FOUND</c> when the id names no domain, or
    /// <c>EMAIL_FORWARDING_UPSTREAM_FAILED</c> when the registrar rejects the update.
    /// </returns>
    public async Task<Result> HandleAsync(SetEmailForwardingCommand cmd, CancellationToken ct)
    {
        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Id == cmd.DomainId!.Value, ct);
        if (domain is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        var result = await _registrar.SetEmailForwardingAsync(domain.Name, cmd.Enabled, ct);
        if (!result.Success)
        {
            return Result.Failure(
                $"Registrar failed to update email forwarding for '{domain.Name}': {result.ErrorMessage}",
                HostpanelErrorCodes.EMAIL_FORWARDING_UPSTREAM_FAILED);
        }

        domain.SetEmailForwarding(cmd.Enabled);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
