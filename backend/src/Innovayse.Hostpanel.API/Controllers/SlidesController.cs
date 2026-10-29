using Innovayse.Hostpanel.Application.Slides.Commands.CreateSlide;
using Innovayse.Hostpanel.Application.Slides.Commands.DeleteSlide;
using Innovayse.Hostpanel.Application.Slides.Commands.UpdateSlide;
using Innovayse.Hostpanel.Application.Slides.Commands.UpdateSlideOrder;
using Innovayse.Hostpanel.Application.Slides.Common;
using Innovayse.Hostpanel.Application.Slides.Queries.GetSlide;
using Innovayse.Hostpanel.Application.Slides.Queries.ListActiveSlides;
using Innovayse.Hostpanel.Application.Slides.Queries.ListSlides;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Filters;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>The homepage slider: the admin panel's editable slides, and the storefront's public feed.</summary>
[Route("api/admin/slides")]
[Authorize(Policy = Policies.Admin)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
public sealed class SlidesController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public SlidesController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Every slide, for the admin panel.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the slides; 401 when the caller is not signed in;
    /// 403 when the caller's role is not allowed;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SlideAdminDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<SlideAdminDto>>>(new ListSlidesQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>A single slide, with every translation.</summary>
    /// <param name="id">The slide's primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the slide; 400 when a value is refused; 404 when the id names no slide;
    /// 401 when the caller is not signed in; 403 when the caller's role is not allowed;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SlideAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<SlideAdminDto>>(new GetSlideQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new homepage slide.</summary>
    /// <param name="command">The slide's fields and its translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 201 with the new slide's id; 400 when a value is refused; 401 when the caller is not signed in;
    /// 403 when the caller's role is not allowed; 404 when the product does not exist;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateSlideCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"/api/admin/slides/{id}", id));
    }

    /// <summary>Updates an existing slide and replaces its translations.</summary>
    /// <param name="id">The slide identifier; wins over any id in the body.</param>
    /// <param name="command">The slide's new fields and translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 when a value is refused; 404 when the id names no slide;
    /// 401 when the caller is not signed in; 403 when the caller's role is not allowed;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateSlideCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Permanently deletes a slide.</summary>
    /// <param name="id">The slide identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 when a value is refused; 404 when the id names no slide;
    /// 401 when the caller is not signed in; 403 when the caller's role is not allowed;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteSlideCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Updates the display order of multiple slides in one operation.</summary>
    /// <param name="command">The slide id and new sort order pairs.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 when a value is refused; 404 when an id names no slide; 401 when the caller is not signed in;
    /// 403 when the caller's role is not allowed;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpPut("order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateOrderAsync(
        [FromBody] UpdateSlideOrderCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command, ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Active slides visible now, localised for the request language (<c>X-Language</c>). Anonymous: the storefront
    /// needs it before anyone has signed in. Which audiences are included is decided on the server from the
    /// caller: an anonymous caller gets <c>All</c> and <c>Guest</c> slides, a signed-in caller gets <c>All</c> and
    /// <c>Authenticated</c> ones.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the visible slides; 429 when the rate limit is hit.</returns>
    [HttpGet("/api/slides")]
    [AllowAnonymous]
    [PublicContentCache]
    [ProducesResponseType(typeof(IReadOnlyList<SlidePublicDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPublicAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<SlidePublicDto>>>(new ListActiveSlidesQuery(), ct);
        return HandleResult(outcome);
    }
}
