using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Queries.GetMyProfile;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Clients.Queries.GetMyContacts;

/// <summary>
/// Reads the caller's contacts from their profile, which already carries them -- see
/// <see cref="ClientDto.Contacts"/> -- rather than querying the table a second way.
/// </summary>
public sealed class GetMyContactsHandler
{
    /// <summary>Wolverine bus used to reach the profile read.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus used to reach the profile read.</param>
    public GetMyContactsHandler(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Handles <see cref="GetMyContactsQuery"/>.</summary>
    /// <param name="query">The query. It names no account: this reads the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The account's contacts, or <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller has no client record.
    /// </returns>
    public async Task<Result<IReadOnlyList<ContactDto>>> HandleAsync(GetMyContactsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var profile = await _bus.InvokeAsync<Result<ClientDto>>(new GetMyProfileQuery(), ct);
        if (profile.IsFailure)
        {
            return Result<IReadOnlyList<ContactDto>>.FailureCode(profile.ErrorCode!);
        }

        return Result<IReadOnlyList<ContactDto>>.Success(profile.Value!.Contacts);
    }
}
