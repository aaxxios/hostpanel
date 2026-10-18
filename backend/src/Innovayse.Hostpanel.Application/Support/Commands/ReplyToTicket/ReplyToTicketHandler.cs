using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.ReplyToTicket;

/// <summary>Posts a staff reply on an existing ticket.</summary>
public sealed class ReplyToTicketHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The signed-in caller; the reply author comes from here, never from the request body.</summary>
    private readonly ICurrentUserService _currentUser;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="currentUser">The signed-in caller; the reply author comes from here.</param>
    public ReplyToTicketHandler(IHostpanelDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The reply to post.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>TICKET_NOT_FOUND</c> when the id names no ticket. Replying to an already
    /// closed ticket is still a domain invariant and keeps throwing
    /// <see cref="InvalidOperationException"/>.
    /// </returns>
    public async Task<Result> HandleAsync(ReplyToTicketCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var ticket = await _context.Tickets
            .Include(t => t.Replies)
            .FirstOrDefaultAsync(t => t.Id == cmd.TicketId, ct);
        if (ticket is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.TICKET_NOT_FOUND);
        }

        // Display name from the token's subject; fall back to the email, then the subject id, so a
        // reply is never recorded against nobody.
        var authorName = _currentUser.UserName;
        if (string.IsNullOrWhiteSpace(authorName))
        {
            authorName = _currentUser.Email;
        }

        if (string.IsNullOrWhiteSpace(authorName))
        {
            authorName = _currentUser.RequireUserId();
        }

        ticket.AddReply(cmd.Message, authorName, cmd.IsStaffReply);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
