using Innovayse.Hostpanel.Application.Admin.Servers.Commands.ConfigureServerSsh;
using Innovayse.Hostpanel.Application.Admin.Servers.Commands.CreateServer;
using Innovayse.Hostpanel.Application.Admin.Servers.Commands.DeleteServer;
using Innovayse.Hostpanel.Application.Admin.Servers.Commands.TestServerSshAgent;
using Innovayse.Hostpanel.Application.Admin.Servers.Commands.UpdateServer;
using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Admin.Servers.Queries.GetServerSshSettings;
using Innovayse.Hostpanel.Application.Admin.Servers.Queries.ListServers;
using Innovayse.Hostpanel.Application.Admin.Servers.Queries.TestServerConnection;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Staff administration of the provisioning servers hosting accounts are placed on.</summary>
[Route("api/admin/servers")]
[Authorize(Policy = Policies.Admin)]
public sealed class ServersController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public ServersController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Every configured provisioning server, optionally filtered by module.</summary>
    /// <param name="query">The optional module filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the servers; 401; 403.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ServerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync([FromQuery] ListServersQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<ServerDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new provisioning server.</summary>
    /// <param name="command">The server's configuration.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new server's location; 400 when a field is refused; 401; 403.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateServerCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"/api/admin/servers/{id}", id));
    }

    /// <summary>Updates an existing server's configuration.</summary>
    /// <param name="id">Server primary key; wins over any id in the body.</param>
    /// <param name="command">The updated configuration.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no server; 400; 401; 403.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        int id, [FromBody] UpdateServerCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Deletes a server.</summary>
    /// <param name="id">Server primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no server; 401; 403.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteServerCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Tests connectivity to a server using its stored credentials and persists the result.</summary>
    /// <param name="id">Server primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the connection test result, whether or not the connection itself succeeded; 404
    /// when the id names no server; 401; 403.
    /// </returns>
    [HttpPost("{id:int}/test-connection")]
    [ProducesResponseType(typeof(TestServerConnectionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TestConnectionAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<TestServerConnectionResultDto>>(
            new TestServerConnectionQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Gets a server's SSH key management settings. The stored private key is never returned.</summary>
    /// <param name="id">Server identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The settings; disabled defaults when never configured.</returns>
    [HttpGet("{id:int}/ssh-settings")]
    [ProducesResponseType(typeof(ServerSshSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSshSettingsAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<ServerSshSettingsDto>>(new GetServerSshSettingsQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates or replaces a server's SSH key management settings.</summary>
    /// <param name="id">Server identifier.</param>
    /// <param name="command">The settings; the server id comes from the route.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The saved settings.</returns>
    [HttpPut("{id:int}/ssh-settings")]
    [ProducesResponseType(typeof(ServerSshSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfigureSshSettingsAsync(
        int id, [FromBody] ConfigureServerSshCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<ServerSshSettingsDto>>(command with { ServerId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Pings the server's SSH key agent and records the health outcome.</summary>
    /// <param name="id">Server identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The outcome.</returns>
    [HttpPost("{id:int}/ssh-settings/test")]
    [ProducesResponseType(typeof(TestServerSshAgentResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TestSshAgentAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<TestServerSshAgentResultDto>>(new TestServerSshAgentCommand(id), ct);
        return HandleResult(outcome);
    }
}
