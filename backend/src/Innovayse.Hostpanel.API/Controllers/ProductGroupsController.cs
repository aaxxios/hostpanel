using Innovayse.Hostpanel.Application.Products.Commands.CreateProductGroup;
using Innovayse.Hostpanel.Application.Products.Commands.UpdateProductGroup;
using Innovayse.Hostpanel.Application.Products.Queries.GetProductGroups;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>The product groups the storefront's catalogue is organized into.</summary>
[Route("api/products/groups")]
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

    /// <summary>Every product group, active-only unless asked otherwise.</summary>
    /// <param name="query">Whether to leave out groups the operator switched off.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the groups.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ProductGroupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync([FromQuery] GetProductGroupsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<ProductGroupDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new product group.</summary>
    /// <param name="command">The group's name and description.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new group's id; 400 when a field is refused; 401; 403.</returns>
    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateProductGroupCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"api/products/groups/{id}", id));
    }

    /// <summary>Updates an existing product group's name and description.</summary>
    /// <param name="id">Product group primary key; wins over any id in the body.</param>
    /// <param name="command">The group's new name and description.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 when a field is refused; 404 with <c>PRODUCT_GROUP_NOT_FOUND</c>; 401; 403.
    /// </returns>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        int id, [FromBody] UpdateProductGroupCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }
}
