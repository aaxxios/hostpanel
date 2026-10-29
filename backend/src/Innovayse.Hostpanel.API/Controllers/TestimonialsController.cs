using Innovayse.Hostpanel.Application.Testimonials.Commands.CreateTestimonial;
using Innovayse.Hostpanel.Application.Testimonials.Commands.DeleteTestimonial;
using Innovayse.Hostpanel.Application.Testimonials.Commands.UpdateTestimonial;
using Innovayse.Hostpanel.Application.Testimonials.Commands.UpdateTestimonialOrder;
using Innovayse.Hostpanel.Application.Testimonials.Common;
using Innovayse.Hostpanel.Application.Testimonials.Queries.GetTestimonial;
using Innovayse.Hostpanel.Application.Testimonials.Queries.ListActiveTestimonials;
using Innovayse.Hostpanel.Application.Testimonials.Queries.ListTestimonials;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Filters;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>The storefront's testimonials: the admin panel's editable list, and the storefront's public feed.</summary>
[Route("api/admin/testimonials")]
[Authorize(Policy = Policies.Admin)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
public sealed class TestimonialsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public TestimonialsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Every testimonial, for the admin panel.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the testimonials; 400 when a value is refused; 401 when the caller is not signed in;
    /// 403 when the caller's role is not allowed;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TestimonialAdminDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<TestimonialAdminDto>>>(new ListTestimonialsQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>A single testimonial, with every translation.</summary>
    /// <param name="id">The testimonial's primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the testimonial; 400 when a value is refused; 404 when the id names no testimonial;
    /// 401 when the caller is not signed in; 403 when the caller's role is not allowed;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TestimonialAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<TestimonialAdminDto>>(new GetTestimonialQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new testimonial.</summary>
    /// <param name="command">The testimonial's fields and its translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 201 with the new testimonial's id; 400 when a value is refused;
    /// 401 when the caller is not signed in; 403 when the caller's role is not allowed;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateTestimonialCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"/api/admin/testimonials/{id}", id));
    }

    /// <summary>Updates an existing testimonial and replaces its translations.</summary>
    /// <param name="id">The testimonial identifier; wins over any id in the body.</param>
    /// <param name="command">The testimonial's new fields and translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 when a value is refused; 404 when the id names no testimonial;
    /// 401 when the caller is not signed in; 403 when the caller's role is not allowed;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateTestimonialCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Permanently deletes a testimonial.</summary>
    /// <param name="id">The testimonial identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 when a value is refused; 404 when the id names no testimonial;
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
        var outcome = await _bus.InvokeAsync<Result>(new DeleteTestimonialCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Updates the display order of several testimonials in one operation.</summary>
    /// <param name="command">The testimonial id and new sort order pairs.</param>
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
    public async Task<IActionResult> UpdateOrderAsync([FromBody] UpdateTestimonialOrderCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command, ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// The active testimonials in display order, localised for the request language (<c>X-Language</c>).
    /// Anonymous: the storefront needs it before anyone has signed in.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the visible testimonials; 400 when a value is refused; 429 when the rate limit is hit.</returns>
    [HttpGet("/api/public/testimonials")]
    [AllowAnonymous]
    [PublicContentCache]
    [ProducesResponseType(typeof(IReadOnlyList<TestimonialPublicDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListPublicAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<TestimonialPublicDto>>>(new ListActiveTestimonialsQuery(), ct);
        return HandleResult(outcome);
    }
}
