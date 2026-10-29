using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Application.Notifications.Queries.GetClientEmailLog;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Notifications.Queries.GetMyEmailLog;

/// <summary>Resolves the caller's client record, then reads one email sent to that client.</summary>
public sealed class GetMyEmailLogHandler
{
    /// <summary>Wolverine bus the caller lookup and the read are dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Resolves the account the caller acts for, owner or linked user.</summary>
    private readonly IClientAccess _access;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus the caller lookup and the read are dispatched through.</param>
    /// <param name="access">Resolves the account the caller acts for, owner or linked user.</param>
    public GetMyEmailLogHandler(IMessageBus bus, IClientAccess access)
    {
        _bus = bus;
        _access = access;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The log entry to read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The email, or <c>CLIENT_PROFILE_NOT_FOUND</c> / <c>EMAIL_LOG_NOT_FOUND</c>.</returns>
    public async Task<Result<MyEmailLogDetailDto>> HandleAsync(GetMyEmailLogQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var clientId = await _access.RequireClientIdAsync(ct);
        if (clientId.IsFailure)
        {
            return Result<MyEmailLogDetailDto>.FailureCode(clientId.ErrorCode!);
        }

        var entry = await _bus.InvokeAsync<Result<EmailLogDetailDto>>(
            new GetClientEmailLogQuery(clientId.Value, query.EmailLogId), ct);
        if (entry.IsFailure)
        {
            return Result<MyEmailLogDetailDto>.FailureCode(entry.ErrorCode!);
        }

        // The delivery error is the mail server's own text, which is for staff only.
        var sent = entry.Value!;
        return Result<MyEmailLogDetailDto>.Success(
            new MyEmailLogDetailDto(sent.Id, sent.To, sent.Subject, sent.Body, sent.SentAt, sent.Success));
    }
}
