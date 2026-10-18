using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Commands.RegisterDomain;

/// <summary>
/// Registers a new domain with the configured registrar provider and persists the resulting aggregate.
/// Calls <see cref="IRegistrarProvider.RegisterAsync"/> first, then creates and activates the domain aggregate.
/// </summary>
public sealed class RegisterDomainHandler
{
    /// <summary>The registrar provider used to place the registration order.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>The data context the new domain aggregate is checked against and persisted through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work the aggregate is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="registrar">The registrar provider used to place the registration order.</param>
    /// <param name="context">The data context the new domain aggregate is checked against and persisted through.</param>
    /// <param name="uow">Unit of work the aggregate is saved through.</param>
    public RegisterDomainHandler(IRegistrarProvider registrar, IHostpanelDbContext context, IUnitOfWork uow)
    {
        _registrar = registrar;
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles <see cref="RegisterDomainCommand"/>.
    /// </summary>
    /// <param name="cmd">The register domain command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The newly created domain ID, or <c>DOMAIN_ALREADY_REGISTERED</c> when the name is already
    /// registered through this system, or <c>DOMAIN_REGISTRATION_UPSTREAM_FAILED</c> when the
    /// registrar rejects the request.
    /// </returns>
    public async Task<Result<int>> HandleAsync(RegisterDomainCommand cmd, CancellationToken ct)
    {
        // Safety net — the availability check should have caught this, but guard against duplicates
        var existing = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Name == cmd.DomainName, ct);
        if (existing is not null)
        {
            return Result.Failure<int>(
                $"Domain '{cmd.DomainName}' is already registered in the system.",
                HostpanelErrorCodes.DOMAIN_ALREADY_REGISTERED);
        }

        var request = new DomainRegistrationOrder(
            cmd.DomainName,
            cmd.Years,
            cmd.WhoisPrivacy,
            cmd.AutoRenew,
            cmd.Nameserver1,
            cmd.Nameserver2);

        var result = await _registrar.RegisterAsync(request, ct);

        if (!result.Success)
        {
            return Result.Failure<int>(
                $"Registrar rejected registration of '{cmd.DomainName}': {result.ErrorMessage}",
                HostpanelErrorCodes.DOMAIN_REGISTRATION_UPSTREAM_FAILED);
        }

        var expiresAt = result.ExpiresAt
            ?? DateTimeOffset.UtcNow.AddYears(cmd.Years);

        var domain = RegisteredDomain.Register(
            cmd.ClientId, cmd.DomainName, expiresAt, cmd.AutoRenew, cmd.WhoisPrivacy,
            recurringAmount: cmd.RecurringAmount,
            firstPaymentAmount: cmd.FirstPaymentAmount,
            registrationPeriod: cmd.Years,
            paymentMethod: cmd.PaymentMethod);

        // When the registrar requires polling (e.g. Name.am with async order processing),
        // leave the domain in PendingRegistration — the SyncPendingDomainStatuses job will
        // activate it once the registrar confirms the domain is live.
        if (!result.RequiresPolling)
        {
            domain.Activate(result.RegistrarRef!);
        }

        _context.Domains.Add(domain);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(domain.Id);
    }
}
