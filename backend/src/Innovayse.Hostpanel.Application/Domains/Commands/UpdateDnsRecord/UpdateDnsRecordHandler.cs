using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Commands.UpdateDnsRecord;

/// <summary>
/// Updates an existing DNS record in the domain aggregate and syncs the change to the registrar.
/// </summary>
public sealed class UpdateDnsRecordHandler
{
    /// <summary>The registrar provider the updated record is synced to.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>The data context the domain aggregate is loaded, mutated and saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work the aggregate is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="registrar">The registrar provider the updated record is synced to.</param>
    /// <param name="context">The data context the domain aggregate is loaded, mutated and saved through.</param>
    /// <param name="uow">Unit of work the aggregate is saved through.</param>
    public UpdateDnsRecordHandler(IRegistrarProvider registrar, IHostpanelDbContext context, IUnitOfWork uow)
    {
        _registrar = registrar;
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles <see cref="UpdateDnsRecordCommand"/>.
    /// </summary>
    /// <param name="cmd">The update DNS record command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>DOMAIN_NOT_FOUND</c> when the id names no domain, or
    /// <c>DNS_RECORD_NOT_FOUND</c> when the record id names no record on that domain, or
    /// <c>DNS_RECORD_UPSTREAM_FAILED</c> when the registrar rejects the update.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateDnsRecordCommand cmd, CancellationToken ct)
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

        domain.UpdateDnsRecord(cmd.RecordId!.Value, cmd.Value, cmd.Ttl, cmd.Priority);

        if (domain.RegistrarRef is not null)
        {
            var updated = domain.DnsRecords.First(r => r.Id == cmd.RecordId!.Value);
            var result = await _registrar.UpdateDnsRecordAsync(domain.Name, domain.RegistrarRef, updated, ct);

            if (!result.Success)
            {
                return Result.Failure(
                    $"Registrar failed to update DNS record {cmd.RecordId} for '{domain.Name}': "
                        + result.ErrorMessage,
                    HostpanelErrorCodes.DNS_RECORD_UPSTREAM_FAILED);
            }
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
