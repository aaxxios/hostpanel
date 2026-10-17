using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Queries.GetClient;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Clients.Queries.GetClientUser;

/// <summary>Loads a client, then the identity account behind it, and assembles the user detail.</summary>
public sealed class GetClientUserHandler
{
    /// <summary>Wolverine bus the client read is dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Reads the people behind client rows.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus the client read is dispatched through.</param>
    /// <param name="identity">Reads the people behind client rows.</param>
    public GetClientUserHandler(IMessageBus bus, IIdentityProvider identity)
    {
        _bus = bus;
        _identity = identity;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The client whose linked person to read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The linked user detail, or <c>CLIENT_NOT_FOUND</c> or <c>IDENTITY_USER_NOT_FOUND</c>.</returns>
    public async Task<Result<UserDetailDto>> HandleAsync(GetClientUserQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var clientOutcome = await _bus.InvokeAsync<Result<ClientDto>>(new GetClientQuery(query.ClientId), ct);
        if (clientOutcome.IsFailure)
        {
            return Result<UserDetailDto>.FailureCode(clientOutcome.ErrorCode!);
        }

        var client = clientOutcome.Value!;
        var account = await _identity.FindBySubjectAsync(client.UserId, ct);
        if (account is null)
        {
            return Result<UserDetailDto>.FailureCode(HostpanelErrorCodes.IDENTITY_USER_NOT_FOUND);
        }

        // Assembled from the account and the client already loaded, rather than from one store
        // that held both. The language comes off the account, which is where it is actually
        // stored; an SSO-backed provider answers null for it, so the screen shows nothing rather
        // than a value hostpanel invented. The timestamp is the client one, which is the one this
        // screen is about.
        return Result<UserDetailDto>.Success(new UserDetailDto(
            account.Subject, account.FirstName, account.LastName, account.Email,
            account.Language, account.LastLoginAt, client.CreatedAt,
            [new UserAccountDto(client.Id, client.FirstName, client.LastName, client.CompanyName, IsOwner: true)]));
    }
}
