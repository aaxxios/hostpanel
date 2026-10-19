using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Faqs.Commands.DeleteFaq;

/// <summary>Deletes a question permanently from the persistence store; its translations go with it.</summary>
public sealed class DeleteFaqHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public DeleteFaqHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The question to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>FAQ_NOT_FOUND</c> when the id names no question.</returns>
    public async Task<Result> HandleAsync(DeleteFaqCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var entity = await _context.Faqs.FirstOrDefaultAsync(x => x.Id == cmd.Id, ct);
        if (entity is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.FAQ_NOT_FOUND);
        }

        _context.Faqs.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
