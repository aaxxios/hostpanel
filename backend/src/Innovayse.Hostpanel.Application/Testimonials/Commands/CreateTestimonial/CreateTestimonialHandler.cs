using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Testimonials.Extensions;
using Innovayse.Hostpanel.Domain.Testimonials;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Testimonials.Commands.CreateTestimonial;

/// <summary>
/// Creates a new <see cref="Testimonial"/> aggregate, applies every translation the command carries, and
/// persists the result.
/// </summary>
public sealed class CreateTestimonialHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public CreateTestimonialHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The testimonial and its translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created testimonial's id. This use case refuses nothing.</returns>
    public async Task<Result<int>> HandleAsync(CreateTestimonialCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var entity = Testimonial.Create(cmd.Name, cmd.Company, cmd.AvatarUrl, cmd.Rating, cmd.SortOrder, cmd.IsActive, cmd.PublishedAt);
        entity.ReplaceTranslations(cmd.Translations.ToContent());

        _context.Testimonials.Add(entity);
        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(entity.Id);
    }
}
