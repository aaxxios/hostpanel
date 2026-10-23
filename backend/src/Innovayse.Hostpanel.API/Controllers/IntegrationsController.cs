using Innovayse.Hostpanel.Application.Admin.Integrations.Commands.SaveIntegrationConfig;
using Innovayse.Hostpanel.Application.Admin.Integrations.Commands.TestIntegrationConnection;
using Innovayse.Hostpanel.Application.Admin.Integrations.Queries.GetHostingPanelV7ServerInfo;
using Innovayse.Hostpanel.Application.Admin.Integrations.Queries.GetHostingPanelServerInfo;
using Innovayse.Hostpanel.Application.Admin.Integrations.Queries.GetIntegration;
using Innovayse.Hostpanel.Application.Admin.Integrations.Queries.ListIntegrations;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Staff administration of third-party integration configurations. Credentials are stored as
/// key-value settings; a secret field is write only and reads back as <c>isSecret</c> and
/// <c>hasValue</c>, never as its value.
/// </summary>
[Route("api/admin/integrations")]
[Authorize(Policy = Policies.Admin)]
public sealed class IntegrationsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public IntegrationsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>The enabled/configured status for every known integration.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the integrations; 401; 403.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<IntegrationListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<IntegrationListItemDto>>>(
            new ListIntegrationsQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>The full configuration for one integration; secret fields carry no value.</summary>
    /// <param name="slug">URL-safe integration identifier, e.g. card.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the integration's detail; 404 when the slug is not recognised; 401; 403.</returns>
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(IntegrationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(string slug, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IntegrationDetailDto>>(new GetIntegrationQuery(slug), ct);
        return HandleResult(outcome);
    }

    /// <summary>Saves (upserts) the configuration for one integration.</summary>
    /// <param name="slug">URL-safe integration identifier; wins over any slug in the body.</param>
    /// <param name="command">
    /// Whether the integration is enabled, its field values and the fields to clear. An empty
    /// secret field keeps the stored value.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 400 when a field is refused; 401; 403.</returns>
    [HttpPut("{slug}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveAsync(
        string slug, [FromBody] SaveIntegrationConfigCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Slug = slug }, ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Tests whether all required credential fields are configured for the integration, and, for
    /// integrations that support it, runs a live probe against the provider.
    /// </summary>
    /// <param name="slug">URL-safe integration identifier, e.g. panel.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the test outcome, whether or not the probe itself succeeded; 404 with
    /// <c>INTEGRATION_NOT_FOUND</c> when the slug is not recognised; 401; 403.
    /// </returns>
    [HttpPost("{slug}/test")]
    [ProducesResponseType(typeof(IntegrationTestResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TestAsync(string slug, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IntegrationTestResultDto>>(
            new TestIntegrationConnectionCommand(slug), ct);
        return HandleResult(outcome);
    }

    /// <summary>Live hosting panel server status (cached 5 minutes).</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the connection status, account count and version; 401; 403.</returns>
    [HttpGet("cwp/server-info")]
    [ProducesResponseType(typeof(HostingPanelServerInfoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHostingPanelServerInfoAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<HostingPanelServerInfoDto>>(new GetHostingPanelServerInfoQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Live hosting panel (v7) server status (cached 5 minutes).</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the connection status and package count; 401; 403.</returns>
    [HttpGet("cwp7/server-info")]
    [ProducesResponseType(typeof(HostingPanelV7ServerInfoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHostingPanelV7ServerInfoAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<HostingPanelV7ServerInfoDto>>(new GetHostingPanelV7ServerInfoQuery(), ct);
        return HandleResult(outcome);
    }
}
