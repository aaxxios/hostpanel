using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Slides.Commands.DeleteSlide;

/// <summary>Deletes a slide permanently from the persistence store.</summary>
public sealed class DeleteSlideHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public DeleteSlideHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The slide to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>SLIDE_NOT_FOUND</c> when the id names no slide.</returns>
    public async Task<Result> HandleAsync(DeleteSlideCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == cmd.Id, ct);
        if (slide is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SLIDE_NOT_FOUND);
        }

        _context.Slides.Remove(slide);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
