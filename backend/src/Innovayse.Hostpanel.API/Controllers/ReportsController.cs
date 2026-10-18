using Innovayse.Hostpanel.Application.Admin.Queries.GetClientGrowthReport;
using Innovayse.Hostpanel.Application.Admin.Queries.GetRevenueReport;
using Innovayse.Hostpanel.Application.Admin.Queries.GetServiceUsageReport;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Admin reporting endpoints for revenue, client growth, and service usage.</summary>
[Route("api/admin/reports")]
[Authorize(Policy = Policies.ResellerOrAdmin)]
public sealed class ReportsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public ReportsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns daily revenue totals for the given date range.</summary>
    /// <param name="query">Start and end of the reporting period.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the daily revenue items; 401; 403.</returns>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(IReadOnlyList<RevenueReportItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueAsync([FromQuery] GetRevenueReportQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<RevenueReportItemDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns daily new client registration counts for the given date range.</summary>
    /// <param name="query">Start and end of the reporting period.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the daily client growth items; 401; 403.</returns>
    [HttpGet("client-growth")]
    [ProducesResponseType(typeof(IReadOnlyList<ClientGrowthReportItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientGrowthAsync(
        [FromQuery] GetClientGrowthReportQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<ClientGrowthReportItemDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns service usage counts grouped by product name.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the usage items, ordered by count descending; 401; 403.</returns>
    [HttpGet("service-usage")]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceUsageReportItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServiceUsageAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<ServiceUsageReportItemDto>>>(
            new GetServiceUsageReportQuery(), ct);
        return HandleResult(outcome);
    }
}
