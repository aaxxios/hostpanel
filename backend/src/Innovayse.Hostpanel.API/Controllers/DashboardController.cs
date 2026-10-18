using Innovayse.Hostpanel.Application.Admin.Queries.GetDashboardStats;
using Innovayse.Hostpanel.Application.Admin.Queries.GetRecentActivity;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Admin dashboard statistics and the notification feed.</summary>
[Route("api/admin/dashboard")]
[Authorize(Policy = Policies.ResellerOrAdmin)]
public sealed class DashboardController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public DashboardController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns aggregated statistics for the admin dashboard.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the statistics; 401; 403.</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatsAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<DashboardStatsDto>>(new GetDashboardStatsQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns the most recent noteworthy events for the notification bell.</summary>
    /// <param name="query">The maximum number of events to return, newest first.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the merged, newest-first activity events; 401; 403.</returns>
    [HttpGet("activity")]
    [ProducesResponseType(typeof(IReadOnlyList<ActivityEventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivityAsync([FromQuery] GetRecentActivityQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<ActivityEventDto>>>(query, ct);
        return HandleResult(outcome);
    }
}
