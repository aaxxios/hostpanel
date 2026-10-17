using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Domains.Commands.TransferDomain;

/// <summary>
/// Initiates an incoming domain transfer via the registrar provider and persists the resulting aggregate.
/// Creates the domain in <c>PendingTransfer</c> status, then transitions it to <c>Active</c> once the
/// registrar confirms.
/// </summary>
public sealed class TransferDomainHandler
{
    /// <summary>The registrar provider used to place the transfer order.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>The data context the new aggregate is persisted through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work the aggregate is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="registrar">The registrar provider used to place the transfer order.</param>
    /// <param name="context">The data context the new aggregate is persisted through.</param>
    /// <param name="uow">Unit of work the aggregate is saved through.</param>
    public TransferDomainHandler(IRegistrarProvider registrar, IHostpanelDbContext context, IUnitOfWork uow)
    {
        _registrar = registrar;
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles <see cref="TransferDomainCommand"/>.
    /// </summary>
    /// <param name="cmd">The transfer domain command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The newly created domain ID, or <c>DOMAIN_TRANSFER_UPSTREAM_FAILED</c> when the registrar
    /// rejects the transfer request.
    /// </returns>
    public async Task<Result<int>> HandleAsync(TransferDomainCommand cmd, CancellationToken ct)
    {
        var request = new DomainTransferOrder(cmd.DomainName, cmd.EppCode, cmd.WhoisPrivacy);
        var result = await _registrar.TransferAsync(request, ct);

        if (!result.Success)
        {
            return Result.Failure<int>(
                $"Registrar rejected transfer of '{cmd.DomainName}': {result.ErrorMessage}",
                HostpanelErrorCodes.DOMAIN_TRANSFER_UPSTREAM_FAILED);
        }

        var domain = RegisteredDomain.CreateTransfer(
            cmd.ClientId, cmd.DomainName,
            firstPaymentAmount: cmd.FirstPaymentAmount,
            recurringAmount: cmd.RecurringAmount,
            paymentMethod: cmd.PaymentMethod);

        // When the registrar requires polling, leave in PendingTransfer.
        if (!result.RequiresPolling)
        {
            var expiresAt = result.ExpiresAt ?? DateTimeOffset.UtcNow.AddYears(1);
            domain.ActivateTransfer(result.RegistrarRef!, expiresAt);
        }

        _context.Domains.Add(domain);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(domain.Id);
    }
}
