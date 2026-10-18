using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Application.Notifications.Queries.ListClientEmailLogs;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Notifications.Queries.ListMyEmailLogs;

/// <summary>Resolves the caller's client record, then reads the mail sent to that client.</summary>
public sealed class ListMyEmailLogsHandler
{
    /// <summary>Wolverine bus the caller lookup and the read are dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus the caller lookup and the read are dispatched through.</param>
    public ListMyEmailLogsHandler(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The page and page size.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The caller's page of log entries, or <c>CLIENT_PROFILE_NOT_FOUND</c>.</returns>
    public async Task<Result<PagedResult<EmailLogDto>>> HandleAsync(ListMyEmailLogsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var profile = await _bus.GetCallerClientAsync(ct);
        if (profile.IsFailure)
        {
            return Result<PagedResult<EmailLogDto>>.FailureCode(profile.ErrorCode!);
        }

        return await _bus.InvokeAsync<Result<PagedResult<EmailLogDto>>>(
            new ListClientEmailLogsQuery(profile.Value!.Id, query.Page, query.PageSize), ct);
    }
}
