using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Commands.AddDnsRecord;

/// <summary>
/// Adds a new DNS record to a domain's zone both in the aggregate and at the registrar.
/// The record is added to the aggregate first so its generated ID is available when calling the registrar.
/// </summary>
public sealed class AddDnsRecordHandler
{
    /// <summary>The registrar provider the new record is synced to.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>The data context the domain aggregate is loaded, mutated and saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work the aggregate is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="registrar">The registrar provider the new record is synced to.</param>
    /// <param name="context">The data context the domain aggregate is loaded, mutated and saved through.</param>
    /// <param name="uow">Unit of work the aggregate is saved through.</param>
    public AddDnsRecordHandler(IRegistrarProvider registrar, IHostpanelDbContext context, IUnitOfWork uow)
    {
        _registrar = registrar;
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles <see cref="AddDnsRecordCommand"/>.
    /// </summary>
    /// <param name="cmd">The add DNS record command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>DOMAIN_NOT_FOUND</c> when the id names no domain, or
    /// <c>DNS_RECORD_UPSTREAM_FAILED</c> when the registrar rejects the addition.
    /// </returns>
    public async Task<Result> HandleAsync(AddDnsRecordCommand cmd, CancellationToken ct)
    {
        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Id == cmd.DomainId!.Value, ct);
        if (domain is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        domain.AddDnsRecord(cmd.Type, cmd.Host, cmd.Value, cmd.Ttl, cmd.Priority);

        if (domain.RegistrarRef is not null)
        {
            var newRecord = domain.DnsRecords[^1];
            var result = await _registrar.AddDnsRecordAsync(domain.Name, domain.RegistrarRef, newRecord, ct);

            if (!result.Success)
            {
                return Result.Failure(
                    $"Registrar failed to add DNS record for '{domain.Name}': {result.ErrorMessage}",
                    HostpanelErrorCodes.DNS_RECORD_UPSTREAM_FAILED);
            }
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
