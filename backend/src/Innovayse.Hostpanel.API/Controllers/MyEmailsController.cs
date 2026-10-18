using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Application.Notifications.Queries.GetMyEmailLog;
using Innovayse.Hostpanel.Application.Notifications.Queries.ListMyEmailLogs;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Client portal endpoints for the emails this account has been sent.
/// </summary>
/// <remarks>
/// The same read as the admin route on <see cref="EmailLogsController"/>, with the client id
/// resolved from the credential rather than taken from the path: a person may read their own
/// correspondence and nobody else's, and an id in the URL is exactly how that goes wrong. Each
/// action dispatches once; the handler behind it resolves the caller and scopes the read.
/// </remarks>
[Route("api/me/emails")]
public sealed class MyEmailsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public MyEmailsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns the emails sent to the authenticated client, newest first.</summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Entries per page; the query clamps this to 1-100.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page; 401 without a session; 404 with <c>CLIENT_PROFILE_NOT_FOUND</c>.</returns>
    [HttpGet]
    [Authorize(Policy = Policies.Client)]
    [ProducesResponseType(typeof(PagedResult<EmailLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyEmailsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var outcome = await _bus.InvokeAsync<Result<PagedResult<EmailLogDto>>>(
            new ListMyEmailLogsQuery(page, pageSize), ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns one email the authenticated client was sent, body included.</summary>
    /// <param name="id">The log entry's primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the email; 404 with <c>EMAIL_LOG_NOT_FOUND</c> when it does not exist or belongs
    /// to another account -- the two are answered alike on purpose, see
    /// <see cref="Application.Notifications.Queries.GetClientEmailLog.GetClientEmailLogHandler"/>;
    /// 401; 404 with <c>CLIENT_PROFILE_NOT_FOUND</c>.
    /// </returns>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.Client)]
    [ProducesResponseType(typeof(EmailLogDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyEmailAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<EmailLogDetailDto>>(new GetMyEmailLogQuery(id), ct);
        return HandleResult(outcome);
    }
}
