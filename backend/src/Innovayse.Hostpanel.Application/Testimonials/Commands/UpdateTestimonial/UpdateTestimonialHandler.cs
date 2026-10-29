using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Testimonials.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Testimonials.Commands.UpdateTestimonial;

/// <summary>
/// Loads an existing testimonial, applies its new field values, replaces its translations, and persists
/// the result.
/// </summary>
public sealed class UpdateTestimonialHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public UpdateTestimonialHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The testimonial's new fields and translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>TESTIMONIAL_NOT_FOUND</c> when the id names no testimonial.</returns>
    public async Task<Result> HandleAsync(UpdateTestimonialCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var entity = await _context.Testimonials.FirstOrDefaultAsync(x => x.Id == cmd.Id, ct);
        if (entity is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.TESTIMONIAL_NOT_FOUND);
        }

        entity.Update(cmd.Name, cmd.Company, cmd.AvatarUrl, cmd.Rating, cmd.SortOrder, cmd.IsActive, cmd.PublishedAt);
        entity.ReplaceTranslations(cmd.Translations.ToContent());

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
