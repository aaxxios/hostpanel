using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Services.Commands.UpdateService;
/// <summary>
/// Handles <see cref="UpdateServiceCommand"/>.
/// Loads the service aggregate, applies editable field changes,
/// and handles status transitions.
/// </summary>
public sealed class UpdateServiceHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work for persisting changes.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="uow">Unit of work for persisting changes.</param>
    public UpdateServiceHandler(IHostpanelDbContext context, IUnitOfWork uow)
    {
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Updates the service's editable fields and optionally changes its status.
    /// </summary>
    /// <param name="cmd">The update command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>SERVICE_NOT_FOUND</c> when the id names no service, or
    /// <c>INVALID_SERVICE_STATUS</c> when <see cref="UpdateServiceCommand.Status"/> names none of
    /// the known statuses -- a defensive answer, since the validator already refuses that shape.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateServiceCommand cmd, CancellationToken ct)
    {
        // Was IClientServiceRepository.FindByIdAsync. Tracked, not AsNoTracking: the service is
        // mutated and saved below.
        var service = await _context.ClientServices.FirstOrDefaultAsync(s => s.Id == cmd.ServiceId, ct);
        if (service is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        service.Update(
            cmd.Domain,
            cmd.DedicatedIp,
            cmd.Username,
            cmd.Password,
            cmd.BillingCycle,
            cmd.RecurringAmount,
            cmd.PaymentMethod,
            cmd.NextRenewalAt,
            cmd.SubscriptionId,
            cmd.OverrideAutoSuspend,
            cmd.SuspendUntil,
            cmd.AutoTerminateEndOfCycle,
            cmd.AutoTerminateReason,
            cmd.AdminNotes,
            cmd.ProvisioningRef,
            cmd.FirstPaymentAmount,
            cmd.PromotionCode,
            cmd.TerminatedAt,
            cmd.ServerId,
            cmd.Quantity,
            cmd.ProductId);

        if (cmd.Status is not null)
        {
            if (!Enum.TryParse<ServiceStatus>(cmd.Status, ignoreCase: true, out var newStatus))
            {
                return Result.FailureCode(HostpanelErrorCodes.INVALID_SERVICE_STATUS);
            }

            if (newStatus != service.Status)
            {
                switch (newStatus)
                {
                    case ServiceStatus.Active:
                        service.Unsuspend();
                        break;
                    case ServiceStatus.Suspended:
                        service.Suspend();
                        break;
                    case ServiceStatus.Terminated:
                        service.Terminate();
                        break;
                    case ServiceStatus.Pending:
                        // Pending is set only on creation — no dedicated domain method
                        break;
                }
            }
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
