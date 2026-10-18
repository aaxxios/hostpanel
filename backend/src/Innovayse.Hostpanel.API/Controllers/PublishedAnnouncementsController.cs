using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Support.Queries.ListPublishedAnnouncements;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// The announcements feed the client portal reads.
/// </summary>
/// <remarks>
/// A separate controller rather than a relaxed authorization on <see cref="AnnouncementsController"/>:
/// that class is admin surface end to end -- create, update, delete, and a read that returns
/// drafts -- and authorization there is declared once for the whole class. Loosening it, even for
/// the one GET, would put a client-reachable action on the same type as the writes. The knowledge
/// base already splits this way (<see cref="KnowledgeBaseController"/> beside
/// <see cref="KnowledgeBaseAdminController"/>), and this follows it.
/// <para>
/// Authenticated rather than anonymous: this is the portal's "Recent News" card, and published
/// does not have to mean world-readable.
/// </para>
/// </remarks>
[Route("api/announcements/published")]
[Authorize(Policy = Policies.Member)]
public sealed class PublishedAnnouncementsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public PublishedAnnouncementsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns a paginated list of published announcements, newest first.</summary>
    /// <param name="query">The paging window.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page, without the editorial published flag.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PublishedAnnouncementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPublishedAsync(
        [FromQuery] ListPublishedAnnouncementsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PagedResult<PublishedAnnouncementDto>>>(query, ct);
        return HandleResult(outcome);
    }
}
