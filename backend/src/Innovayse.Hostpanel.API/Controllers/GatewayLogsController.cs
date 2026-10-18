using Innovayse.Hostpanel.Application.Billing.Queries.ListGatewayLogs;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Admin view of the payment-gateway log.</summary>
[Route("api/gateway-logs")]
public sealed class GatewayLogsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public GatewayLogsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns a paginated, filtered page of the payment-gateway log, newest first.</summary>
    /// <param name="query">Paging and filters (date range, gateway, debug text, result).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page; 400 when paging is out of range; 401 without a session; 403 for a caller who is not an admin.</returns>
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(PagedResult<GatewayLogEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListAsync([FromQuery] ListGatewayLogsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PagedResult<GatewayLogEntryDto>>>(query, ct);
        return HandleResult(outcome);
    }
}
