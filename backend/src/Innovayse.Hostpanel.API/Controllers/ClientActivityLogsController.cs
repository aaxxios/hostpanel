using Innovayse.Hostpanel.Application.Audit.Queries.ListClientActivityLogs;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Admin endpoints for viewing the per-client activity log.</summary>
[Route("api/clients/{clientId:int}/activity-logs")]
[Authorize(Policy = Policies.ResellerOrAdmin)]
public sealed class ClientActivityLogsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public ClientActivityLogsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns a paginated, filtered list of activity log entries for a specific client.</summary>
    /// <param name="clientId">FK to the client; wins over any client id in the query.</param>
    /// <param name="query">The page, page size and optional filters to list by.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the page of activity log entries; 401 without a session; 403 for a caller who is
    /// not staff.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ActivityLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsync(
        int clientId, [FromQuery] ListClientActivityLogsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PagedResult<ActivityLogDto>>>(
            query with { ClientId = clientId }, ct);
        return HandleResult(outcome);
    }
}
