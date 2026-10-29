using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Provisioning.Common;
using Innovayse.Hostpanel.Application.Provisioning.Common.Enums;
using Innovayse.Hostpanel.Application.Provisioning.Extensions;
using Innovayse.Hostpanel.Domain.Provisioning;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Infrastructure.Provisioning;

/// <summary>Default <see cref="IHostedAccountOperations"/>.</summary>
public sealed class HostedAccountOperations : IHostedAccountOperations
{
    /// <summary>Reads the server the service's account lives on from the Admin module.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the provisioning provider configured for that server.</summary>
    private readonly IProvisioningProviderFactory _providerFactory;

    /// <summary>Records a panel failure, whose detail never reaches a caller.</summary>
    private readonly ILogger<HostedAccountOperations> _logger;

    /// <summary>Initializes the operations.</summary>
    /// <param name="bus">Reads the server the service's account lives on from the Admin module.</param>
    /// <param name="providerFactory">Creates the provisioning provider configured for that server.</param>
    /// <param name="logger">Records a panel failure, whose detail never reaches a caller.</param>
    public HostedAccountOperations(
        IMessageBus bus,
        IProvisioningProviderFactory providerFactory,
        ILogger<HostedAccountOperations> logger)
    {
        _bus = bus;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<HostedAccountOutcome> SuspendAsync(HostedAccountSuspension input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        return RunAsync(
            input.Account,
            (provider, provisioningRef) => provider.SuspendAsync(
                new SuspendAccountOrder(input.Account.ServiceId, provisioningRef, input.Reason), ct),
            HostedAccountOperation.Suspend,
            ct);
    }

    /// <inheritdoc/>
    public Task<HostedAccountOutcome> UnsuspendAsync(HostedAccountUnsuspension input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        return RunAsync(
            input.Account,
            (provider, provisioningRef) => provider.UnsuspendAsync(provisioningRef, ct),
            HostedAccountOperation.Unsuspend,
            ct);
    }

    /// <inheritdoc/>
    public Task<HostedAccountOutcome> TerminateAsync(HostedAccountTermination input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        return RunAsync(
            input.Account,
            (provider, provisioningRef) => provider.TerminateAsync(
                new TerminateAccountOrder(input.Account.ServiceId, provisioningRef, input.Reason), ct),
            HostedAccountOperation.Terminate,
            ct);
    }

    /// <summary>Finds the account's server, sends one call to its panel and logs a failure.</summary>
    /// <param name="account">The account acted on.</param>
    /// <param name="call">The provider call, given the provider and the account's provisioning reference.</param>
    /// <param name="operation">Which operation this is, for the failure log line.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The outcome of the call.</returns>
    private async Task<HostedAccountOutcome> RunAsync(
        HostedAccountTarget account,
        Func<IProvisioningProvider, string, Task> call,
        HostedAccountOperation operation,
        CancellationToken ct)
    {
        if (account.ProvisioningRef is not { } provisioningRef)
        {
            return HostedAccountOutcome.NoAccount;
        }

        var server = await _bus.FindServerAsync(account.ServerId, ct);
        if (server is null)
        {
            return HostedAccountOutcome.NoAccount;
        }

        try
        {
            await call(_providerFactory.CreateFor(server), provisioningRef);
            return HostedAccountOutcome.Updated;
        }
        catch (Exception ex)
        {
            // A provider counts an account already in the wanted state as success, so what lands
            // here is a real failure: access denied, an unreachable host or a module with no
            // provider. It is logged rather than swallowed, and the provider's own message stays
            // in the log and never reaches a caller.
            _logger.LogWarning(
                ex,
                "{Operation} of service {ServiceId} on server {ServerId} failed; the hosting account may be out of step with the platform.",
                operation,
                account.ServiceId,
                server.Id);
            return HostedAccountOutcome.Failed;
        }
    }
}
