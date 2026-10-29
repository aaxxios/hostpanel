using Innovayse.Hostpanel.Application.Support.Commands.CreateDownload;
using Innovayse.Hostpanel.Application.Support.Commands.CreateDownloadCategory;
using Innovayse.Hostpanel.Application.Support.Commands.DeleteDownload;
using Innovayse.Hostpanel.Application.Support.Commands.DeleteDownloadCategory;
using Innovayse.Hostpanel.Application.Support.Commands.UpdateDownload;
using Innovayse.Hostpanel.Application.Support.Commands.UpdateDownloadCategory;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Queries.GetDownload;
using Innovayse.Hostpanel.Application.Support.Queries.ListDownloadCategories;
using Innovayse.Hostpanel.Application.Support.Queries.ListDownloads;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Admin endpoints for managing downloadable files and download categories.</summary>
[Route("api/downloads")]
[Authorize(Policy = Policies.Admin)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
public sealed class DownloadsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public DownloadsController(IMessageBus bus)
    {
        _bus = bus;
    }

    // ── Categories ──────────────────────────────────────────────────────

    /// <summary>Returns all download categories with download counts.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the category list; 401 without a session; 403 for a caller who is not an admin; 429 when the rate limit is hit.</returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<DownloadCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListCategoriesAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<DownloadCategoryDto>>>(
            new ListDownloadCategoriesQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new download category.</summary>
    /// <param name="command">The category's details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new category's id; 404 when the parent category names no category; 401 without a session; 403 for a caller who is not an admin; 400 when the body, query or route fails validation; 429 when the rate limit is hit.</returns>
    [HttpPost("categories")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCategoryAsync(
        [FromBody] CreateDownloadCategoryCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult("/api/downloads/categories", id));
    }

    /// <summary>Updates an existing download category.</summary>
    /// <param name="id">Category primary key; wins over any id in the body.</param>
    /// <param name="command">The updated category details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id, or the parent category, names no category; 401 without a session; 403 for a caller who is not an admin; 400 when the body, query or route fails validation; 429 when the rate limit is hit.</returns>
    [HttpPut("categories/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategoryAsync(
        int id, [FromBody] UpdateDownloadCategoryCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Deletes a download category.</summary>
    /// <param name="id">Category primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no category; 409 with <c>DOWNLOAD_CATEGORY_IN_USE</c> when it still has sub-categories or downloads; 401 without a session; 403 for a caller who is not an admin; 429 when the rate limit is hit.</returns>
    [HttpDelete("categories/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCategoryAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteDownloadCategoryCommand(id), ct);
        return HandleResult(outcome);
    }

    // ── Downloads ───────────────────────────────────────────────────────

    /// <summary>Returns downloads, optionally filtered by category.</summary>
    /// <param name="query">The optional category filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the download list; 400 when the body, query or route fails validation; 401 without a session; 403 for a caller who is not an admin; 429 when the rate limit is hit.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DownloadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync([FromQuery] ListDownloadsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<DownloadDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a single download by id.</summary>
    /// <param name="id">Download primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the download; 404 when the id names no download; 401 without a session; 403 for a caller who is not an admin; 429 when the rate limit is hit.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DownloadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<DownloadDto>>(new GetDownloadQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new download entry.</summary>
    /// <param name="command">The download's details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new download's id; 404 when the category names no category; 401 without a session; 403 for a caller who is not an admin; 400 when the body, query or route fails validation; 429 when the rate limit is hit.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateDownloadCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"/api/downloads/{id}", id));
    }

    /// <summary>Updates an existing download entry.</summary>
    /// <param name="id">Download primary key; wins over any id in the body.</param>
    /// <param name="command">The updated download details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no download, or the category names no category; 401 without a session; 403 for a caller who is not an admin; 400 when the body, query or route fails validation; 429 when the rate limit is hit.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateDownloadCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Deletes a download entry.</summary>
    /// <param name="id">Download primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no download; 401 without a session; 403 for a caller who is not an admin; 429 when the rate limit is hit.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteDownloadCommand(id), ct);
        return HandleResult(outcome);
    }
}
