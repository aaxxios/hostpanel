using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Admin.Servers.Queries.GetServerConnection;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Services.Commands.UpdateServiceHostingAccount;

/// <summary>Handles <see cref="UpdateServiceHostingAccountCommand"/>: records the hosting account details on the service.</summary>
public sealed class UpdateServiceHostingAccountHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The message bus, used to ask the servers module whether the chosen server exists.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">The message bus.</param>
    public UpdateServiceHostingAccountHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>Records the hosting account details on the service.</summary>
    /// <param name="cmd">The hosting account details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>SERVICE_NOT_FOUND</c> when the id names no service, or <c>SERVER_NOT_FOUND</c> when the
    /// chosen server does not exist.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateServiceHostingAccountCommand cmd, CancellationToken ct)
    {
        var service = await _context.ClientServices.FirstOrDefaultAsync(s => s.Id == cmd.ServiceId, ct);
        if (service is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        if (cmd.ServerId is int serverId)
        {
            var server = await _bus.InvokeAsync<Result<ServerConnectionDto>>(new GetServerConnectionQuery(serverId), ct);
            if (!server.IsSuccess)
            {
                return Result.FailureCode(HostpanelErrorCodes.SERVER_NOT_FOUND);
            }
        }

        service.RecordHostingAccount(new ServiceHostingAccount(
            cmd.ServerId,
            cmd.Domain,
            cmd.ProvisioningRef,
            cmd.DedicatedIp,
            cmd.Username,
            cmd.Password));

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
