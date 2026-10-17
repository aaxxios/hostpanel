using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.DeletePredefinedReply;

/// <summary>Deletes a predefined reply.</summary>
public sealed class DeletePredefinedReplyHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public DeletePredefinedReplyHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The reply to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>PREDEFINED_REPLY_NOT_FOUND</c> when the id names no reply.</returns>
    public async Task<Result> HandleAsync(DeletePredefinedReplyCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var reply = await _context.PredefinedReplies.FirstOrDefaultAsync(r => r.Id == cmd.Id, ct);
        if (reply is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.PREDEFINED_REPLY_NOT_FOUND);
        }

        _context.PredefinedReplies.Remove(reply);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
