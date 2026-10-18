using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreatePredefinedReply;

/// <summary>Creates a new predefined reply.</summary>
public sealed class CreatePredefinedReplyHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CreatePredefinedReplyHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The reply's details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the new reply's id. This use case answers no refusal code.</returns>
    public async Task<Result<int>> HandleAsync(CreatePredefinedReplyCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var reply = PredefinedReply.Create(cmd.Name, cmd.Content, cmd.CategoryId);

        _context.PredefinedReplies.Add(reply);
        await _context.SaveChangesAsync(ct);

        return Result.Success(reply.Id);
    }
}
