using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Testimonials.Common;
using Innovayse.Hostpanel.Application.Testimonials.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Testimonials.Queries.GetTestimonial;

/// <summary>Loads a single testimonial with every translation and maps it to <see cref="TestimonialAdminDto"/>.</summary>
public sealed class GetTestimonialHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public GetTestimonialHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The testimonial id to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The admin DTO, or <c>TESTIMONIAL_NOT_FOUND</c> when the id names no testimonial.</returns>
    public async Task<Result<TestimonialAdminDto>> HandleAsync(GetTestimonialQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var entity = await _context.Testimonials.AsNoTracking().FirstOrDefaultAsync(x => x.Id == query.Id, ct);
        if (entity is null)
        {
            return Result<TestimonialAdminDto>.FailureCode(HostpanelErrorCodes.TESTIMONIAL_NOT_FOUND);
        }

        return Result<TestimonialAdminDto>.Success(entity.ToAdminDto());
    }
}
