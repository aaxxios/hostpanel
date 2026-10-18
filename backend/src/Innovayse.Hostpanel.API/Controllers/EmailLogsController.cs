using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Application.Notifications.Queries.ListClientEmailLogs;
using Innovayse.Hostpanel.Application.Notifications.Queries.ListEmailLogs;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Admin endpoints for the platform's outbound email delivery log.</summary>
[Route("api/admin/email-logs")]
[Authorize(Policy = Policies.Admin)]
public sealed class EmailLogsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public EmailLogsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns a page of email log entries, newest first.</summary>
    /// <param name="query">The requested page and page size.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page; 401; 403.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EmailLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync([FromQuery] ListEmailLogsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<EmailLogDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a page of email logs sent to a specific client.</summary>
    /// <param name="clientId">Client primary key; wins over any id in the query string.</param>
    /// <param name="query">The requested page and page size.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page, empty when the client id names nobody; 401; 403.</returns>
    [HttpGet("client/{clientId:int}")]
    [ProducesResponseType(typeof(PagedResult<EmailLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListByClientAsync(
        int clientId, [FromQuery] ListClientEmailLogsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PagedResult<EmailLogDto>>>(query with { ClientId = clientId }, ct);
        return HandleResult(outcome);
    }
}
