using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Domains.Commands.UpdateDomain;

/// <summary>
/// Applies administrative and billing field updates to a domain aggregate and persists changes.
/// Optionally replaces the nameserver list if non-empty nameservers are provided.
/// </summary>
public sealed class UpdateDomainHandler
{
    /// <summary>The data context the domain aggregate is loaded, mutated and saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work the aggregate is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context the domain aggregate is loaded, mutated and saved through.</param>
    /// <param name="uow">Unit of work the aggregate is saved through.</param>
    public UpdateDomainHandler(IHostpanelDbContext context, IUnitOfWork uow)
    {
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles <see cref="UpdateDomainCommand"/>.
    /// </summary>
    /// <param name="cmd">The update domain command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>DOMAIN_NOT_FOUND</c> when the id names no domain.</returns>
    public async Task<Result> HandleAsync(UpdateDomainCommand cmd, CancellationToken ct)
    {
        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Id == cmd.DomainId!.Value, ct);
        if (domain is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        var status = Enum.Parse<DomainStatus>(cmd.Status);
        var expiresAt = DateTimeOffset.Parse(cmd.ExpiresAt);
        var nextDueDate = DateTimeOffset.Parse(cmd.NextDueDate);

        domain.Update(
            cmd.FirstPaymentAmount, cmd.RecurringAmount, cmd.PaymentMethod,
            cmd.PromotionCode, cmd.SubscriptionId, cmd.AdminNotes,
            expiresAt, nextDueDate, cmd.RegistrationPeriod, status);

        var nameserverHosts = cmd.Nameservers
            .Where(ns => !string.IsNullOrWhiteSpace(ns))
            .ToList();

        if (nameserverHosts.Count > 0)
        {
            domain.SetNameservers(nameserverHosts);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
