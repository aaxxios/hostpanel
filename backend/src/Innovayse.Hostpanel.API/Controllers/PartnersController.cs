using Innovayse.Hostpanel.Application.Partners.Commands.CreatePartner;
using Innovayse.Hostpanel.Application.Partners.Commands.DeletePartner;
using Innovayse.Hostpanel.Application.Partners.Commands.UpdatePartner;
using Innovayse.Hostpanel.Application.Partners.Commands.UpdatePartnerOrder;
using Innovayse.Hostpanel.Application.Partners.Common;
using Innovayse.Hostpanel.Application.Partners.Queries.GetPartner;
using Innovayse.Hostpanel.Application.Partners.Queries.ListActivePartners;
using Innovayse.Hostpanel.Application.Partners.Queries.ListPartners;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Filters;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>The storefront's partners: the admin panel's editable list, and the storefront's public feed.</summary>
[Route("api/admin/partners")]
[Authorize(Policy = Policies.Admin)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
public sealed class PartnersController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public PartnersController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Every partner, for the admin panel.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the partners; 400 with <c>VALIDATION_ERROR</c>; 401 without a session; 403 for a caller
    /// who is not an administrator;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PartnerAdminDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<PartnerAdminDto>>>(new ListPartnersQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>A single partner, with every translation.</summary>
    /// <param name="id">The partner's primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the partner; 400 with <c>VALIDATION_ERROR</c> for a non-positive id; 401 without a
    /// session; 403 for a caller who is not an administrator; 404 with <c>PARTNER_NOT_FOUND</c>;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PartnerAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PartnerAdminDto>>(new GetPartnerQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new partner.</summary>
    /// <param name="command">The partner's fields and its translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 201 with the new partner's id; 400 with <c>VALIDATION_ERROR</c> when a field is refused; 401 without a session
    /// without a session; 403 for a caller who is not an administrator;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync([FromBody] CreatePartnerCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"/api/admin/partners/{id}", id));
    }

    /// <summary>Updates an existing partner and replaces its translations.</summary>
    /// <param name="id">The partner identifier; wins over any id in the body.</param>
    /// <param name="command">The partner's new fields and translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 with <c>VALIDATION_ERROR</c> when a field is refused; 401 without a session;
    /// 403 for a caller who is not an administrator; 404 with <c>PARTNER_NOT_FOUND</c>;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdatePartnerCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Permanently deletes a partner.</summary>
    /// <param name="id">The partner identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 with <c>VALIDATION_ERROR</c> for a non-positive id; 401 without a session;
    /// 403 for a caller who is not an administrator; 404 with <c>PARTNER_NOT_FOUND</c>;
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
        var outcome = await _bus.InvokeAsync<Result>(new DeletePartnerCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Updates the display order of several partners in one operation.</summary>
    /// <param name="command">The partner id and new sort order pairs.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 with <c>VALIDATION_ERROR</c> when a field is refused; 401 without a session;
    /// 403 for a caller who is not an administrator;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpPut("order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateOrderAsync([FromBody] UpdatePartnerOrderCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command, ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// The active partners in display order, localised for the request language (<c>X-Language</c>).
    /// Anonymous: the storefront needs it before anyone has signed in.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the active partners; 400 with <c>VALIDATION_ERROR</c>;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpGet("/api/public/partners")]
    [AllowAnonymous]
    [PublicContentCache]
    [ProducesResponseType(typeof(IReadOnlyList<PartnerPublicDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListPublicAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<PartnerPublicDto>>>(new ListActivePartnersQuery(), ct);
        return HandleResult(outcome);
    }
}
