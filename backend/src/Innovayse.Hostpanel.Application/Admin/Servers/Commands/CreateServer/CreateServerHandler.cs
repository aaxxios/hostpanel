using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.CreateServer;

/// <summary>
/// Handles <see cref="CreateServerCommand"/> by creating and persisting a new provisioning server.
/// </summary>
public sealed class CreateServerHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CreateServerHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates the server and returns its assigned identifier.
    /// </summary>
    /// <param name="cmd">The server's configuration.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The new server's identifier. Always succeeds once validation has passed.</returns>
    public async Task<Result<int>> HandleAsync(CreateServerCommand cmd, CancellationToken ct)
    {
        var details = new ServerDetails(
            cmd.Name, cmd.Hostname, cmd.IpAddress, cmd.AssignedIpAddresses,
            cmd.Module, cmd.Username, cmd.Password, cmd.ApiToken, cmd.AccessHash,
            cmd.UseSSL, cmd.MaxAccounts, cmd.IsDefault, cmd.IsDisabled,
            cmd.MonthlyCost, cmd.Datacenter, cmd.ServerStatusAddress,
            cmd.Ns1Hostname, cmd.Ns1Ip, cmd.Ns2Hostname, cmd.Ns2Ip,
            cmd.Ns3Hostname, cmd.Ns3Ip, cmd.Ns4Hostname, cmd.Ns4Ip,
            cmd.Ns5Hostname, cmd.Ns5Ip);

        var server = Server.Create(details);
        _context.Servers.Add(server);
        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(server.Id);
    }
}
