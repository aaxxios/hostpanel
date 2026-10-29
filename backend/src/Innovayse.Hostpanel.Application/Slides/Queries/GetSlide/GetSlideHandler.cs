using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Products.Common;
using Innovayse.Hostpanel.Application.Products.Queries.GetProductSummaries;
using Innovayse.Hostpanel.Application.Slides.Common;
using Innovayse.Hostpanel.Application.Slides.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Slides.Queries.GetSlide;

/// <summary>
/// Loads a single slide, asks the Products module for its linked product's name if it has one, and
/// maps it to <see cref="SlideAdminDto"/>.
/// </summary>
public sealed class GetSlideHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Dispatches the Products query that names the linked product; this module does not read product tables.</summary>
    private readonly IMessageBus _bus;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">Dispatches the Products query that names the linked product.</param>
    public GetSlideHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The slide id to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The admin DTO, or <c>SLIDE_NOT_FOUND</c> when the id names no slide.</returns>
    public async Task<Result<SlideAdminDto>> HandleAsync(GetSlideQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var slide = await _context.Slides.AsNoTracking().FirstOrDefaultAsync(s => s.Id == query.Id, ct);
        if (slide is null)
        {
            return Result<SlideAdminDto>.FailureCode(HostpanelErrorCodes.SLIDE_NOT_FOUND);
        }

        string? productName = null;
        if (slide.ProductId.HasValue)
        {
            var products = await _bus.InvokeAsync<Result<IReadOnlyList<ProductSummaryDto>>>(
                new GetProductSummariesQuery([slide.ProductId.Value]), ct);
            productName = products.Value?.FirstOrDefault()?.Name;
        }

        return Result<SlideAdminDto>.Success(slide.ToAdminDto(productName));
    }
}
