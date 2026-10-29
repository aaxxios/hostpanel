using Innovayse.Hostpanel.Application.Faqs.Commands.CreateFaq;
using Innovayse.Hostpanel.Application.Faqs.Commands.DeleteFaq;
using Innovayse.Hostpanel.Application.Faqs.Commands.UpdateFaq;
using Innovayse.Hostpanel.Application.Faqs.Commands.UpdateFaqOrder;
using Innovayse.Hostpanel.Application.Faqs.Common;
using Innovayse.Hostpanel.Application.Faqs.Queries.GetFaq;
using Innovayse.Hostpanel.Application.Faqs.Queries.ListActiveFaqs;
using Innovayse.Hostpanel.Application.Faqs.Queries.ListFaqs;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Filters;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>The storefront's questions: the admin panel's editable list, and the storefront's public feed.</summary>
[Route("api/admin/faqs")]
[Authorize(Policy = Policies.Admin)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
public sealed class FaqsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public FaqsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Every question, for the admin panel.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the questions; 401 without a session; 403 for a caller who is not an admin; 429 when the rate limit is hit.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FaqAdminDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<FaqAdminDto>>>(new ListFaqsQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>A single question, with every translation.</summary>
    /// <param name="id">The question's primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the question; 404 with <c>FAQ_NOT_FOUND</c>; 401 without a session; 403 for a caller who is not an admin; 400 when the request is refused;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FaqAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<FaqAdminDto>>(new GetFaqQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new question.</summary>
    /// <param name="command">The question's fields and its translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new question's id; 400 when a field is refused; 401 without a session; 403 for a caller who is not an admin; 429 when the rate limit is hit.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateFaqCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"/api/admin/faqs/{id}", id));
    }

    /// <summary>Updates an existing question and replaces its translations.</summary>
    /// <param name="id">The question identifier; wins over any id in the body.</param>
    /// <param name="command">The question's new fields and translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 400 when a field is refused; 404 with <c>FAQ_NOT_FOUND</c>; 401 without a session; 403 for a caller who is not an admin; 429 when the rate limit is hit.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateFaqCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Permanently deletes a question.</summary>
    /// <param name="id">The question identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 404 with <c>FAQ_NOT_FOUND</c>; 401 without a session; 403 for a caller who is not an admin; 400 when the request is refused;
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
        var outcome = await _bus.InvokeAsync<Result>(new DeleteFaqCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Updates the display order of several questions in one operation.</summary>
    /// <param name="command">The question id and new sort order pairs.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 400 when a field is refused; 404 when an id names no row; 401 without a session; 403 for a caller who is not an admin; 429 when the rate limit is hit.</returns>
    [HttpPut("order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateOrderAsync([FromBody] UpdateFaqOrderCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command, ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// The active questions in display order, localised for the request language (<c>X-Language</c>).
    /// Anonymous: the storefront needs it before anyone has signed in.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the active questions; 429 when the rate limit is hit.</returns>
    [HttpGet("/api/public/faqs")]
    [AllowAnonymous]
    [PublicContentCache]
    [ProducesResponseType(typeof(IReadOnlyList<FaqPublicDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPublicAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<FaqPublicDto>>>(new ListActiveFaqsQuery(), ct);
        return HandleResult(outcome);
    }
}
