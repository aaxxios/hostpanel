using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Testimonials.Common;
using Innovayse.Hostpanel.Application.Testimonials.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Testimonials.Queries.ListTestimonials;

/// <summary>Loads every testimonial with its translations and maps them to <see cref="TestimonialAdminDto"/>.</summary>
public sealed class ListTestimonialsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public ListTestimonialsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The query; it carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every testimonial, ordered by sort order ascending. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<TestimonialAdminDto>>> HandleAsync(ListTestimonialsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await _context.Testimonials.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync(ct);
        return Result<IReadOnlyList<TestimonialAdminDto>>.Success(rows.Select(x => x.ToAdminDto()).ToList());
    }
}
