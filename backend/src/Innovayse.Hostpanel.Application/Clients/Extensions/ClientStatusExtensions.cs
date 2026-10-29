using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Clients.Extensions;

/// <summary>
/// What a client's account status allows, in one place for every module that orders, bills or
/// takes payment: a suspended or closed account does none of those, an active or inactive one does.
/// </summary>
public static class ClientStatusExtensions
{
    /// <summary>Whether the account may be billed and may place orders: every status but Suspended and Closed.</summary>
    /// <param name="status">The account's status.</param>
    /// <returns><see langword="true"/> for Active and Inactive.</returns>
    public static bool MayTransact(this ClientStatus status) => status is ClientStatus.Active or ClientStatus.Inactive;

    /// <summary>The refusal for an account that may not order, pay online or be billed.</summary>
    /// <param name="status">The account's status.</param>
    /// <returns>
    /// Success for Active and Inactive, <c>CLIENT_SUSPENDED</c> for a suspended account and
    /// <c>CLIENT_CLOSED</c> for a closed one.
    /// </returns>
    public static Result EnsureMayTransact(this ClientStatus status) => status switch
    {
        ClientStatus.Suspended => Result.FailureCode(HostpanelErrorCodes.CLIENT_SUSPENDED),
        ClientStatus.Closed => Result.FailureCode(HostpanelErrorCodes.CLIENT_CLOSED),
        _ => Result.Success(),
    };

    /// <summary>Reads the client's status from the Clients module and applies <see cref="EnsureMayTransact"/>.</summary>
    /// <param name="bus">The Wolverine bus the client's brief is read through.</param>
    /// <param name="clientId">The client record's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success when the account may transact, <c>CLIENT_NOT_FOUND</c> when the id names no record,
    /// or the refusal of <see cref="EnsureMayTransact"/>.
    /// </returns>
    public static async Task<Result> EnsureClientMayTransactAsync(this IMessageBus bus, int clientId, CancellationToken ct)
    {
        var client = await bus.FindClientBriefAsync(clientId, withEmail: false, ct);

        return client is null
            ? Result.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND)
            : client.Status.EnsureMayTransact();
    }
}
