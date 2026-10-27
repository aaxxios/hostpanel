using Innovayse.Hostpanel.Application.Domains.Commands.CreateTldConfig;
using Innovayse.Hostpanel.Application.Domains.Commands.DeleteTldConfig;
using Innovayse.Hostpanel.Application.Domains.Commands.ImportTldPricing;
using Innovayse.Hostpanel.Application.Domains.Commands.SyncTldCostPrices;
using Innovayse.Hostpanel.Application.Domains.Commands.UpdateTldConfig;
using Innovayse.Hostpanel.Application.Domains.Queries.GetTldConfig;
using Innovayse.Hostpanel.Application.Domains.Queries.ListTldConfigs;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;
/// <summary>
/// Admin endpoints for managing TLD configurations (CRUD, import from registrars, and cost-price sync).
/// </summary>
[Route("api/admin/tld-configs")]
public sealed class TldConfigsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public TldConfigsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns all TLD configurations for the admin list view.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with every TLD configuration and its summary pricing; 401; 403.</returns>
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(List<TldConfigListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<List<TldConfigListItemDto>>>(new ListTldConfigsQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a single TLD configuration by ID with full pricing details.</summary>
    /// <param name="id">The TLD configuration identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the configuration; 404 with <c>TLD_CONFIG_NOT_FOUND</c>; 401; 403.</returns>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(TldConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<TldConfigDto>>(new GetTldConfigQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new TLD configuration.</summary>
    /// <param name="command">The create TLD configuration command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 201 with the new configuration's id; 400 with <c>TLD_REGISTRAR_MODULE_UNKNOWN</c>; 409 with
    /// <c>TLD_CONFIG_ALREADY_EXISTS</c>; 401; 403.
    /// </returns>
    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateTldConfigCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"api/admin/tld-configs/{id}", id));
    }

    /// <summary>Updates an existing TLD configuration's settings and pricing.</summary>
    /// <param name="id">The TLD configuration identifier from the route; wins over any id in the body.</param>
    /// <param name="command">The update TLD configuration command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 404 with <c>TLD_CONFIG_NOT_FOUND</c>; 400 with
    /// <c>TLD_REGISTRAR_MODULE_UNKNOWN</c>; 401; 403.
    /// </returns>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateTldConfigCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Permanently deletes a TLD configuration by ID.</summary>
    /// <param name="id">The TLD configuration identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 with <c>TLD_CONFIG_NOT_FOUND</c>; 401; 403.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteTldConfigCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Imports TLD pricing from the specified registrar module, creating or updating TLD configurations.</summary>
    /// <param name="module">The registrar module name to import from (e.g. "NameAm", "Namecheap").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the import counts; 400 with <c>TLD_REGISTRAR_MODULE_UNKNOWN</c>; 401; 403.
    /// </returns>
    [HttpPost("import/{module}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(ImportTldPricingResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportAsync(string module, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<ImportTldPricingResult>>(new ImportTldPricingCommand(module), ct);
        return HandleResult(outcome);
    }

    /// <summary>Triggers an immediate sync of registrar cost prices for all TLD configurations.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 401; 403.</returns>
    [HttpPost("sync")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SyncAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new SyncTldCostPricesNowCommand(), ct);
        return HandleResult(outcome);
    }
}
