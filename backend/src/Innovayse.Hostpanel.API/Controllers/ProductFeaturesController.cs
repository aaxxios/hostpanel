using Innovayse.Hostpanel.Application.Products.Commands.CreateProductFeature;
using Innovayse.Hostpanel.Application.Products.Commands.DeleteProductFeature;
using Innovayse.Hostpanel.Application.Products.Commands.UpdateProductFeature;
using Innovayse.Hostpanel.Application.Products.Queries.GetProductFeatures;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>The specification lines shown in the storefront's plan comparison table.</summary>
[Route("api/product-features")]
public sealed class ProductFeaturesController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public ProductFeaturesController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Specification lines, filtered by group or by product.</summary>
    /// <param name="query">The group/product filters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the lines.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ProductFeatureDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync([FromQuery] GetProductFeaturesQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<ProductFeatureDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Adds a specification line to a product.</summary>
    /// <param name="command">The line's product, label, value and position.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 201 with the new line's id; 400 with <c>PRODUCT_FEATURE_LABEL_DUPLICATE</c> or a field
    /// refusal; 404 with <c>PRODUCT_NOT_FOUND</c>; 401; 403.
    /// </returns>
    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateProductFeatureCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"api/product-features/{id}", id));
    }

    /// <summary>Updates a specification line.</summary>
    /// <param name="id">Feature line primary key; wins over any id in the body.</param>
    /// <param name="command">The line's new label, value and position.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 with <c>PRODUCT_FEATURE_LABEL_DUPLICATE</c> or a field refusal; 404 with
    /// <c>PRODUCT_FEATURE_NOT_FOUND</c>; 401; 403.
    /// </returns>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        int id, [FromBody] UpdateProductFeatureCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Removes a specification line.</summary>
    /// <param name="id">Feature line primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 with <c>PRODUCT_FEATURE_NOT_FOUND</c>; 401; 403.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteProductFeatureCommand(id), ct);
        return HandleResult(outcome);
    }
}
