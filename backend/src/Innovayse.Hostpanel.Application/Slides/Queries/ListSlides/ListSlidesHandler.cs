using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Products.Common;
using Innovayse.Hostpanel.Application.Products.Queries.GetProductSummaries;
using Innovayse.Hostpanel.Application.Slides.Common;
using Innovayse.Hostpanel.Application.Slides.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Slides.Queries.ListSlides;

/// <summary>
/// Loads every slide, asks the Products module for the linked products' names in one dispatch, and
/// maps them to <see cref="SlideAdminDto"/>.
/// </summary>
public sealed class ListSlidesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Dispatches the Products query that names the linked products; this module does not read product tables.</summary>
    private readonly IMessageBus _bus;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">Dispatches the Products query that names the linked products.</param>
    public ListSlidesHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The query; it carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every slide, ordered by sort order ascending. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<SlideAdminDto>>> HandleAsync(ListSlidesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var slides = await _context.Slides.AsNoTracking().OrderBy(s => s.SortOrder).ThenBy(s => s.Id).ToListAsync(ct);

        var productIds = slides
            .Where(s => s.ProductId.HasValue)
            .Select(s => s.ProductId!.Value)
            .Distinct()
            .ToList();
        var products = await _bus.InvokeAsync<Result<IReadOnlyList<ProductSummaryDto>>>(
            new GetProductSummariesQuery(productIds), ct);
        var productNames = (products.Value ?? []).ToDictionary(p => p.Id, p => p.Name);

        var dtos = slides
            .Select(s => s.ToAdminDto(
                s.ProductId.HasValue && productNames.TryGetValue(s.ProductId.Value, out var name) ? name : null))
            .ToList();
        return Result<IReadOnlyList<SlideAdminDto>>.Success(dtos);
    }
}
