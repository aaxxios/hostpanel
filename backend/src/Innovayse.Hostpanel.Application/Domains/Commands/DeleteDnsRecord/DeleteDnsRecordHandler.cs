using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Commands.DeleteDnsRecord;

/// <summary>
/// Removes a DNS record from the domain aggregate and deletes it at the registrar.
/// </summary>
public sealed class DeleteDnsRecordHandler
{
    /// <summary>The registrar provider the deletion is synced to.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>The data context the domain aggregate is loaded, mutated and saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work the aggregate is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="registrar">The registrar provider the deletion is synced to.</param>
    /// <param name="context">The data context the domain aggregate is loaded, mutated and saved through.</param>
    /// <param name="uow">Unit of work the aggregate is saved through.</param>
    public DeleteDnsRecordHandler(IRegistrarProvider registrar, IHostpanelDbContext context, IUnitOfWork uow)
    {
        _registrar = registrar;
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles <see cref="DeleteDnsRecordCommand"/>.
    /// </summary>
    /// <param name="cmd">The delete DNS record command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>DOMAIN_NOT_FOUND</c> when the id names no domain, or
    /// <c>DNS_RECORD_NOT_FOUND</c> when the record id names no record on that domain, or
    /// <c>DNS_RECORD_UPSTREAM_FAILED</c> when the registrar rejects the deletion.
    /// </returns>
    public async Task<Result> HandleAsync(DeleteDnsRecordCommand cmd, CancellationToken ct)
    {
        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Id == cmd.DomainId!.Value, ct);
        if (domain is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        if (domain.DnsRecords.All(r => r.Id != cmd.RecordId!.Value))
        {
            return Result.FailureCode(HostpanelErrorCodes.DNS_RECORD_NOT_FOUND);
        }

        domain.RemoveDnsRecord(cmd.RecordId!.Value);

        if (domain.RegistrarRef is not null)
        {
            var result = await _registrar.DeleteDnsRecordAsync(
                domain.Name, domain.RegistrarRef, cmd.RecordId!.Value, ct);

            if (!result.Success)
            {
                return Result.Failure(
                    $"Registrar failed to delete DNS record {cmd.RecordId} for '{domain.Name}': "
                        + result.ErrorMessage,
                    HostpanelErrorCodes.DNS_RECORD_UPSTREAM_FAILED);
            }
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
