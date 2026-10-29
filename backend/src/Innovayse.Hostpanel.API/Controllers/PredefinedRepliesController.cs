using Innovayse.Hostpanel.Application.Support.Commands.CreatePredefinedReply;
using Innovayse.Hostpanel.Application.Support.Commands.CreatePredefinedReplyCategory;
using Innovayse.Hostpanel.Application.Support.Commands.DeletePredefinedReply;
using Innovayse.Hostpanel.Application.Support.Commands.DeletePredefinedReplyCategory;
using Innovayse.Hostpanel.Application.Support.Commands.UpdatePredefinedReply;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Queries.ListPredefinedReplies;
using Innovayse.Hostpanel.Application.Support.Queries.ListPredefinedReplyCategories;
using Innovayse.Hostpanel.Application.Support.Queries.SearchPredefinedReplies;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Admin endpoints for managing predefined reply categories and replies.</summary>
[Route("api/predefined-replies")]
[Authorize(Policy = Policies.Admin)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
public sealed class PredefinedRepliesController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public PredefinedRepliesController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns all predefined reply categories with reply counts.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the category list; 401 without a session; 403 for a caller who is not an administrator;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<PredefinedReplyCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListCategoriesAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<PredefinedReplyCategoryDto>>>(
            new ListPredefinedReplyCategoriesQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new predefined reply category.</summary>
    /// <param name="command">The category's details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 201 with the new category's id; 400 with <c>VALIDATION_ERROR</c> when a field is refused; 401 without a session
    /// without a session; 403 for a caller who is not an administrator; 404 with
    /// <c>PREDEFINED_REPLY_CATEGORY_NOT_FOUND</c> when the parent category names no category;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpPost("categories")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCategoryAsync(
        [FromBody] CreatePredefinedReplyCategoryCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult("/api/predefined-replies/categories", id));
    }

    /// <summary>Permanently deletes a predefined reply category.</summary>
    /// <param name="id">Category primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 with <c>VALIDATION_ERROR</c> for a non-positive id; 401 without a session;
    /// 403 for a caller who is not an administrator; 404 with
    /// <c>PREDEFINED_REPLY_CATEGORY_NOT_FOUND</c> when the id names no category; 409 with
    /// <c>PREDEFINED_REPLY_CATEGORY_IN_USE</c> when it still has sub-categories or replies;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpDelete("categories/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCategoryAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeletePredefinedReplyCategoryCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns all predefined replies, optionally filtered by category.</summary>
    /// <param name="query">The optional category filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the reply list; 400 with <c>VALIDATION_ERROR</c> for a bad category filter; 401 without
    /// a session; 403 for a caller who is not an administrator;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PredefinedReplyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync([FromQuery] ListPredefinedRepliesQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<PredefinedReplyDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Searches predefined replies by name or content.</summary>
    /// <param name="query">The search term.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the matching replies; 400 with <c>VALIDATION_ERROR</c> when the term is empty; 401 without a session
    /// without a session; 403 for a caller who is not an administrator;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<PredefinedReplyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchAsync([FromQuery] SearchPredefinedRepliesQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<PredefinedReplyDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new predefined reply.</summary>
    /// <param name="command">The reply's details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 201 with the new reply's id; 400 with <c>VALIDATION_ERROR</c> when a field is refused; 401 without a session
    /// without a session; 403 for a caller who is not an administrator; 404 with
    /// <c>PREDEFINED_REPLY_CATEGORY_NOT_FOUND</c> when the category names no category;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAsync([FromBody] CreatePredefinedReplyCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult("/api/predefined-replies", id));
    }

    /// <summary>Updates an existing predefined reply.</summary>
    /// <param name="id">Reply primary key; wins over any id in the body.</param>
    /// <param name="command">The updated reply fields.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 with <c>VALIDATION_ERROR</c> when a field is refused; 401 without a session;
    /// 403 for a caller who is not an administrator; 404 with <c>PREDEFINED_REPLY_NOT_FOUND</c> when
    /// the id names no reply, or <c>PREDEFINED_REPLY_CATEGORY_NOT_FOUND</c> when the category names no category;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdatePredefinedReplyCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Permanently deletes a predefined reply.</summary>
    /// <param name="id">Reply primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 with <c>VALIDATION_ERROR</c> for a non-positive id; 401 without a session;
    /// 403 for a caller who is not an administrator; 404 with <c>PREDEFINED_REPLY_NOT_FOUND</c> when
    /// the id names no reply;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeletePredefinedReplyCommand(id), ct);
        return HandleResult(outcome);
    }
}
