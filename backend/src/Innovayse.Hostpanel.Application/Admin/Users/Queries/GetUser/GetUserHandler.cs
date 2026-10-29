using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Admin.Users.Queries.GetUser;

/// <summary>
/// Handles <see cref="GetUserQuery"/>: composes one person's identity record with the client
/// account linked to them, if any.
/// </summary>
public sealed class GetUserHandler
{
    /// <summary>Reads people from wherever they live.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Wolverine bus, used to find the client account a person owns through the Clients module.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="identity">Reads people from wherever they live.</param>
    /// <param name="bus">Wolverine bus, used to find the client account a person owns through the Clients module.</param>
    public GetUserHandler(IIdentityProvider identity, IMessageBus bus)
    {
        _identity = identity;
        _bus = bus;
    }

    /// <summary>Returns the user.</summary>
    /// <param name="query">The subject to look up.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The user detail, or <c>IDENTITY_USER_NOT_FOUND</c> when the identity provider does not know
    /// the subject.
    /// </returns>
    public async Task<Result<UserDetailDto>> HandleAsync(GetUserQuery query, CancellationToken ct)
    {
        var account = await _identity.FindBySubjectAsync(query.Id, ct);
        if (account is null)
        {
            return Result<UserDetailDto>.FailureCode(HostpanelErrorCodes.IDENTITY_USER_NOT_FOUND);
        }

        var client = await _bus.FindClientBriefBySubjectAsync(query.Id, ct);
        var accounts = client is not null
            ? new List<UserAccountDto>
            {
                new(client.Id, client.FirstName, client.LastName, client.CompanyName, IsOwner: true),
            }
            : [];

        var dto = new UserDetailDto(
            account.Subject, account.FirstName, account.LastName, account.Email,
            // Language lives with whichever store holds the person: a local deployment
            // answers with the one on its own account row, a provider-backed one answers null
            // because the sign-on provider does not hand it out. Null rather than guessed at either way.
            account.Language, account.LastLoginAt,
            client?.CreatedAt ?? default, accounts);

        return Result<UserDetailDto>.Success(dto);
    }
}
