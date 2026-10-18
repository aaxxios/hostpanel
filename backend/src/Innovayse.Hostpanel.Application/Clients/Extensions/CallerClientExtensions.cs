using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Queries.GetMyProfile;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Clients.Extensions;

/// <summary>Resolves the caller's own client record for the <c>My*</c> handlers that need its id.</summary>
public static class CallerClientExtensions
{
    /// <summary>Resolves the client record behind the credential of the current request.</summary>
    /// <param name="bus">The Wolverine bus to dispatch the profile query through.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The caller's profile, or the <c>CLIENT_PROFILE_NOT_FOUND</c> refusal.</returns>
    public static Task<Result<ClientDto>> GetCallerClientAsync(this IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(bus);

        return bus.InvokeAsync<Result<ClientDto>>(new GetMyProfileQuery(), ct);
    }
}
