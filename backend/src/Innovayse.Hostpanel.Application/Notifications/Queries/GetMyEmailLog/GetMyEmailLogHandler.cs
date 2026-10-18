using Innovayse.Hostpanel.Application.Clients.Extensions;
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

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus the caller lookup and the read are dispatched through.</param>
    public GetMyEmailLogHandler(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The log entry to read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The email, or <c>CLIENT_PROFILE_NOT_FOUND</c> / <c>EMAIL_LOG_NOT_FOUND</c>.</returns>
    public async Task<Result<EmailLogDetailDto>> HandleAsync(GetMyEmailLogQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var profile = await _bus.GetCallerClientAsync(ct);
        if (profile.IsFailure)
        {
            return Result<EmailLogDetailDto>.FailureCode(profile.ErrorCode!);
        }

        return await _bus.InvokeAsync<Result<EmailLogDetailDto>>(
            new GetClientEmailLogQuery(profile.Value!.Id, query.EmailLogId), ct);
    }
}
