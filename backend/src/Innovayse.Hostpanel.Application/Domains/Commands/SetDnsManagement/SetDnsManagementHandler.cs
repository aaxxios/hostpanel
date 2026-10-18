using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Commands.SetDnsManagement;

/// <summary>
/// Enables or disables DNS management for a domain at both the registrar and aggregate levels.
/// </summary>
public sealed class SetDnsManagementHandler
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
    public SetDnsManagementHandler(IRegistrarProvider registrar, IHostpanelDbContext context, IUnitOfWork uow)
    {
        _registrar = registrar;
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles <see cref="SetDnsManagementCommand"/>.
    /// </summary>
    /// <param name="cmd">The set DNS management command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>DOMAIN_NOT_FOUND</c> when the id names no domain, or
    /// <c>DNS_MANAGEMENT_UPSTREAM_FAILED</c> when the registrar rejects the update.
    /// </returns>
    public async Task<Result> HandleAsync(SetDnsManagementCommand cmd, CancellationToken ct)
    {
        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Id == cmd.DomainId!.Value, ct);
        if (domain is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        var result = await _registrar.SetDnsManagementAsync(domain.Name, cmd.Enabled, ct);
        if (!result.Success)
        {
            return Result.Failure(
                $"Registrar failed to update DNS management for '{domain.Name}': {result.ErrorMessage}",
                HostpanelErrorCodes.DNS_MANAGEMENT_UPSTREAM_FAILED);
        }

        domain.SetDnsManagement(cmd.Enabled);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
