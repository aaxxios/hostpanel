using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdatePredefinedReply;

/// <summary>Updates an existing predefined reply.</summary>
public sealed class UpdatePredefinedReplyHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdatePredefinedReplyHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The reply to update and its new fields.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>PREDEFINED_REPLY_NOT_FOUND</c> when the id names no reply.</returns>
    public async Task<Result> HandleAsync(UpdatePredefinedReplyCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var reply = await _context.PredefinedReplies.FirstOrDefaultAsync(r => r.Id == cmd.Id, ct);
        if (reply is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.PREDEFINED_REPLY_NOT_FOUND);
        }

        reply.Update(cmd.Name, cmd.Content, cmd.CategoryId);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
