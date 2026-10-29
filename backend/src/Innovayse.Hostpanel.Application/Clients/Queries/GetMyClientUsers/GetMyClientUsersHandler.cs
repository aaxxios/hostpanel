using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Queries.GetClientUsers;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Clients.Queries.GetMyClientUsers;

/// <summary>Resolves the caller's client record, then lists the users linked to it.</summary>
public sealed class GetMyClientUsersHandler
{
    /// <summary>Wolverine bus the caller lookup and the read are dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Resolves the account the caller acts for (the owner only).</summary>
    private readonly IClientAccess _access;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus the caller lookup and the read are dispatched through.</param>
    /// <param name="access">Resolves the account the caller acts for (the owner only).</param>
    public GetMyClientUsersHandler(IMessageBus bus, IClientAccess access)
    {
        _bus = bus;
        _access = access;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query; it carries nothing.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The account's users, or <c>CLIENT_PROFILE_NOT_FOUND</c>.</returns>
    public async Task<Result<IReadOnlyList<ClientUserDto>>> HandleAsync(GetMyClientUsersQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var clientId = await _access.RequireOwnClientIdAsync(ct);
        if (clientId.IsFailure)
        {
            return Result<IReadOnlyList<ClientUserDto>>.FailureCode(clientId.ErrorCode!);
        }

        return await _bus.InvokeAsync<Result<IReadOnlyList<ClientUserDto>>>(
            new GetClientUsersQuery(clientId.Value), ct);
    }
}
