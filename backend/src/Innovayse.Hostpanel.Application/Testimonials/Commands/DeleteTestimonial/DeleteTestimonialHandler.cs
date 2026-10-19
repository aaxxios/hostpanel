using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Testimonials.Commands.DeleteTestimonial;

/// <summary>Deletes a testimonial permanently from the persistence store; its translations go with it.</summary>
public sealed class DeleteTestimonialHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public DeleteTestimonialHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The testimonial to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>TESTIMONIAL_NOT_FOUND</c> when the id names no testimonial.</returns>
    public async Task<Result> HandleAsync(DeleteTestimonialCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var entity = await _context.Testimonials.FirstOrDefaultAsync(x => x.Id == cmd.Id, ct);
        if (entity is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.TESTIMONIAL_NOT_FOUND);
        }

        _context.Testimonials.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
