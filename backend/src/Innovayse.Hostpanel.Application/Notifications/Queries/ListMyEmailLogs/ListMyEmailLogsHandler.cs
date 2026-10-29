using Innovayse.Hostpanel.Application.Common.Interfaces;
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

    /// <summary>Resolves the account the caller acts for, owner or linked user.</summary>
    private readonly IClientAccess _access;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus the caller lookup and the read are dispatched through.</param>
    /// <param name="access">Resolves the account the caller acts for, owner or linked user.</param>
    public ListMyEmailLogsHandler(IMessageBus bus, IClientAccess access)
    {
        _bus = bus;
        _access = access;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The page and page size.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The caller's page of log entries without the delivery error, or <c>CLIENT_PROFILE_NOT_FOUND</c>.</returns>
    public async Task<Result<PaginatedList<MyEmailLogDto>>> HandleAsync(ListMyEmailLogsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var clientId = await _access.RequireClientIdAsync(ct);
        if (clientId.IsFailure)
        {
            return Result<PaginatedList<MyEmailLogDto>>.FailureCode(clientId.ErrorCode!);
        }

        var page = await _bus.InvokeAsync<Result<PaginatedList<EmailLogDto>>>(
            new ListClientEmailLogsQuery(clientId.Value, query.Page, query.PageSize), ct);
        if (page.IsFailure)
        {
            return Result<PaginatedList<MyEmailLogDto>>.FailureCode(page.ErrorCode!);
        }

        // The delivery error is the mail server's own text, which is for staff only; a client
        // sees whether delivery succeeded and nothing about the server that answered.
        var list = page.Value!;
        return Result<PaginatedList<MyEmailLogDto>>.Success(new PaginatedList<MyEmailLogDto>(
            list.Items.Select(entry => new MyEmailLogDto(entry.Id, entry.To, entry.Subject, entry.SentAt, entry.Success)).ToList(),
            list.TotalCount,
            list.PageNumber,
            list.PageSize));
    }
}
