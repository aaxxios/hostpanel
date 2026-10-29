using Innovayse.Hostpanel.Application.Products.Commands.CreateProductGroup;
using Innovayse.Hostpanel.Application.Products.Commands.UpdateProductGroup;
using Innovayse.Hostpanel.Application.Products.Common;
using Innovayse.Hostpanel.Application.Products.Queries.GetProductGroups;
using Innovayse.Hostpanel.Application.Products.Queries.ListCatalogProductGroups;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>The product groups the catalogue is organized into: the storefront's list and the admin panel's editable rows.</summary>
[Route("api/products/groups")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
public sealed class ProductGroupsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public ProductGroupsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>
    /// The staff view of the product groups, active-only unless asked otherwise. Lists groups the
    /// operator switched off when asked, so it is for staff only.
    /// </summary>
    /// <param name="query">Whether to leave out groups the operator switched off.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the groups; 400 when a filter is refused; 401 without a session; 403 for a caller who is not an admin; 429 when the rate limit is hit.</returns>
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(IReadOnlyList<ProductGroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync([FromQuery] GetProductGroupsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<ProductGroupDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// The storefront catalogue's product groups: active groups only, with no staff field.
    /// Anonymous: the storefront renders them before anyone has signed in.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the active groups; 429 when the rate limit is hit.</returns>
    [HttpGet("/api/public/product-groups")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<CatalogProductGroupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListCatalogAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<CatalogProductGroupDto>>>(
            new ListCatalogProductGroupsQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new product group.</summary>
    /// <param name="command">The group's name and description.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new group's id; 400 when a field is refused; 401 without a session; 403 for a caller who is not an admin; 429 when the rate limit is hit.</returns>
    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateProductGroupCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"/api/products/groups/{id}", id));
    }

    /// <summary>Updates an existing product group's name and description.</summary>
    /// <param name="id">Product group primary key; wins over any id in the body.</param>
    /// <param name="command">The group's new name and description.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 when a field is refused; 404 with <c>PRODUCT_GROUP_NOT_FOUND</c>; 401 without a session; 403 for a caller who is not an admin;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateAsync(
        int id, [FromBody] UpdateProductGroupCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }
}
