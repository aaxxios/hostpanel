using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Support.Commands.CreateAnnouncement;
using Innovayse.Hostpanel.Application.Support.Commands.DeleteAnnouncement;
using Innovayse.Hostpanel.Application.Support.Commands.UpdateAnnouncement;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Queries.GetAnnouncement;
using Innovayse.Hostpanel.Application.Support.Queries.ListAnnouncements;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Admin endpoints for managing announcements: create, update, delete, and reads that may
/// return unpublished drafts.
/// </summary>
[Route("api/announcements")]
[Authorize(Policy = Policies.Admin)]
public sealed class AnnouncementsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public AnnouncementsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns a paginated list of all announcements, drafts included.</summary>
    /// <param name="query">The paging window.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AnnouncementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync([FromQuery] ListAnnouncementsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PagedResult<AnnouncementDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a single announcement by its identifier.</summary>
    /// <param name="id">Announcement primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the announcement; 404 when the id names no announcement.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AnnouncementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<AnnouncementDto>>(new GetAnnouncementQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new announcement.</summary>
    /// <param name="command">Announcement creation details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new announcement's id.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateAnnouncementCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"api/announcements/{id}", id));
    }

    /// <summary>Updates an existing announcement.</summary>
    /// <param name="id">Announcement primary key; wins over any id in the body.</param>
    /// <param name="command">Updated announcement details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no announcement.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateAnnouncementCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Permanently deletes an announcement.</summary>
    /// <param name="id">Announcement primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no announcement.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteAnnouncementCommand(id), ct);
        return HandleResult(outcome);
    }
}
