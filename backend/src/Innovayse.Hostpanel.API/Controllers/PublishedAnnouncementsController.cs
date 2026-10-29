using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Queries.GetPublishedAnnouncement;
using Innovayse.Hostpanel.Application.Support.Queries.ListPublishedAnnouncements;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Filters;
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
/// that class is admin surface end to end, create, update, delete, and a read that returns
/// drafts, and authorization there is declared once for the whole class. Loosening it, even for
/// the one GET, would put a client-reachable action on the same type as the writes. The knowledge
/// base already splits this way (<see cref="KnowledgeBaseController"/> beside
/// <see cref="KnowledgeBaseAdminController"/>), and this follows it.
/// <para>
/// The list is authenticated rather than anonymous: it is the portal's "Recent News" card (the client
/// announcements page shows a sign-in prompt to a signed-out visitor), and published
/// does not have to mean world-readable. The detail is public, because a published announcement is
/// linked by address and the id of a draft answers 404 exactly like an unknown id.
/// </para>
/// </remarks>
[Route("api/announcements/published")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
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
    /// <returns>200 with the page, without the editorial published flag; 400 when the paging window is invalid; 401 without a session; 429 when the rate limit is hit.</returns>
    [HttpGet]
    [PublicContentCache]
    [ProducesResponseType(typeof(PaginatedList<PublishedAnnouncementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListPublishedAsync(
        [FromQuery] ListPublishedAnnouncementsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PaginatedList<PublishedAnnouncementDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns one published announcement, body included.</summary>
    /// <param name="id">The announcement identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the announcement; 400 when the id is not positive; 404 when the id names no published announcement (a draft answers the same); 429 when the rate limit is hit.</returns>
    [HttpGet("/api/public/announcements/{id:int}")]
    [AllowAnonymous]
    [PublicContentCache]
    [ProducesResponseType(typeof(PublishedAnnouncementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublishedAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PublishedAnnouncementDto>>(new GetPublishedAnnouncementQuery(id), ct);
        return HandleResult(outcome);
    }
}
