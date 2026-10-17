using Innovayse.Hostpanel.Application.Products.Commands.CreateProduct;
using Innovayse.Hostpanel.Application.Products.Commands.UpdateProduct;
using Innovayse.Hostpanel.Application.Products.Common;
using Innovayse.Hostpanel.Application.Products.Queries.GetProducts;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>The catalogue's products: the storefront's plans and the admin panel's editable rows.</summary>
[Route("api/products")]
public sealed class ProductsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public ProductsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Products, optionally filtered by group or active status, priced in the caller's currency.</summary>
    /// <param name="query">The group/active filters and the requested currency.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the products.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync([FromQuery] GetProductsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<ProductDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new product in a product group.</summary>
    /// <param name="command">The product's details and prices.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 201 with the new product's id; 400 when a field is refused; 404 with
    /// <c>PRODUCT_GROUP_NOT_FOUND</c>; 401; 403.
    /// </returns>
    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateProductCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"api/products/{id}", id));
    }

    /// <summary>Updates an existing product's details and prices.</summary>
    /// <param name="id">Product primary key; wins over any id in the body.</param>
    /// <param name="command">The product's new details and prices.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 when a field is refused; 404 with <c>PRODUCT_NOT_FOUND</c>; 401; 403.
    /// </returns>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        int id, [FromBody] UpdateProductCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }
}
