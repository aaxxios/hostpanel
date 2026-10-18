using Innovayse.Hostpanel.Application.Slides.Commands.CreateSlide;
using Innovayse.Hostpanel.Application.Slides.Commands.DeleteSlide;
using Innovayse.Hostpanel.Application.Slides.Commands.UpdateSlide;
using Innovayse.Hostpanel.Application.Slides.Commands.UpdateSlideOrder;
using Innovayse.Hostpanel.Application.Slides.Commands.UploadSlideImage;
using Innovayse.Hostpanel.Application.Slides.Common;
using Innovayse.Hostpanel.Application.Slides.Queries.GetSlide;
using Innovayse.Hostpanel.Application.Slides.Queries.ListActiveSlides;
using Innovayse.Hostpanel.Application.Slides.Queries.ListSlides;
using Innovayse.Hostpanel.Domain.Slides;
using Innovayse.Hostpanel.Domain.Slides.Enums;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>The homepage slider: the admin panel's editable slides, and the storefront's public feed.</summary>
[Route("api/admin/slides")]
[Authorize(Policy = Policies.Admin)]
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
    /// <returns>200 with the slides; 401; 403.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SlideAdminDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<SlideAdminDto>>>(new ListSlidesQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>A single slide, with every translation.</summary>
    /// <param name="id">The slide's primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the slide; 404 with <c>SLIDE_NOT_FOUND</c>; 401; 403.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SlideAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<SlideAdminDto>>(new GetSlideQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new homepage slide.</summary>
    /// <param name="command">The slide's fields and its translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new slide's id; 400 when a field is refused; 401; 403.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateSlideCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"api/admin/slides/{id}", id));
    }

    /// <summary>Updates an existing slide and replaces its translations.</summary>
    /// <param name="id">The slide identifier; wins over any id in the body.</param>
    /// <param name="command">The slide's new fields and translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 when a field is refused; 404 with <c>SLIDE_NOT_FOUND</c>; 401; 403.
    /// </returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateSlideCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Permanently deletes a slide.</summary>
    /// <param name="id">The slide identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 with <c>SLIDE_NOT_FOUND</c>; 401; 403.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteSlideCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Updates the display order of multiple slides in one operation.</summary>
    /// <param name="command">The slide id and new sort order pairs.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 400 when a field is refused; 401; 403.</returns>
    [HttpPut("order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOrderAsync(
        [FromBody] UpdateSlideOrderCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command, ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Uploads a slide image and stores it locally. Not one of the Result-returning commands
    /// against the database; this is a file write, and it answers a <c>Result&lt;SlideImageDto&gt;</c>
    /// of its own kind through the same <c>HandleResult</c> path as everything else.
    /// </summary>
    /// <param name="file">The image file to upload; its stream, length, name and media type go to the command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the uploaded image's URL; 400 with <c>SLIDE_IMAGE_TYPE_INVALID</c>,
    /// <c>SLIDE_IMAGE_EXCEEDS_LIMIT</c> or a field refusal; 401; 403.
    /// </returns>
    [HttpPost("upload-image")]
    [ProducesResponseType(typeof(SlideImageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImageAsync(IFormFile file, CancellationToken ct)
    {
        await using var content = file?.OpenReadStream() ?? Stream.Null;
        var command = new UploadSlideImageCommand(
            content, file?.Length ?? 0, file?.ContentType ?? string.Empty, file?.FileName ?? string.Empty);
        var outcome = await _bus.InvokeAsync<Result<SlideImageDto>>(command, ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Active slides visible now, localised for the request language (<c>X-Language</c>). Anonymous: the storefront
    /// needs it before anyone has signed in.
    /// </summary>
    /// <param name="audience">Optional audience filter (<c>All</c>, <c>Guest</c> or <c>Authenticated</c>); absent returns every audience.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the visible slides.</returns>
    [HttpGet("/api/slides")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<SlidePublicDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPublicAsync([FromQuery] SlideAudience? audience, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<SlidePublicDto>>>(
            new ListActiveSlidesQuery(audience), ct);
        return HandleResult(outcome);
    }
}
