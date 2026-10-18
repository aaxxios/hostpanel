using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Notifications.Queries.GetClientEmailLog;

/// <summary>Returns one email a client was sent.</summary>
public sealed class GetClientEmailLogHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the client's sign-in address; a log entry is keyed by address, not by id.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="identity">Resolves the client's sign-in address.</param>
    public GetClientEmailLogHandler(IHostpanelDbContext context, IIdentityProvider identity)
    {
        _context = context;
        _identity = identity;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The entry, or <c>EMAIL_LOG_NOT_FOUND</c> when it does not exist, was sent to another
    /// client, or the client id itself names nobody -- all answered alike deliberately: telling
    /// them apart would confirm that an entry exists to whoever guessed its id.
    /// </returns>
    public async Task<Result<EmailLogDetailDto>> HandleAsync(GetClientEmailLogQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == query.ClientId, ct);
        var account = client is null ? null : await _identity.FindBySubjectAsync(client.UserId, ct);
        if (account is null)
        {
            return Result<EmailLogDetailDto>.FailureCode(HostpanelErrorCodes.EMAIL_LOG_NOT_FOUND);
        }

        var log = await _context.EmailLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == query.EmailLogId && l.To == account.Email, ct);
        if (log is null)
        {
            return Result<EmailLogDetailDto>.FailureCode(HostpanelErrorCodes.EMAIL_LOG_NOT_FOUND);
        }

        return Result<EmailLogDetailDto>.Success(
            new EmailLogDetailDto(log.Id, log.To, log.Subject, log.Body, log.SentAt, log.Success, log.Error));
    }
}
