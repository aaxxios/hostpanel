using Innovayse.Hostpanel.API.Authorization;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Application.Clients.Commands.InviteMyUser;
using Innovayse.Hostpanel.Application.Clients.Commands.RemoveMyUser;
using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Queries.GetMyClientUsers;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Client portal endpoints for the people linked to the caller's own account.</summary>
/// <remarks>
/// The equivalent admin routes live on <see cref="ClientsController"/> and take the client id in
/// the path. Here it is resolved from the credential instead, so a client can only ever list,
/// invite and remove people on their own account.
/// </remarks>
[Route("api/me/users")]
public sealed class MyUsersController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public MyUsersController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Lists the people linked to the authenticated client's own account.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the account's users; 401; 404 with <c>CLIENT_PROFILE_NOT_FOUND</c>;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks full access (<c>All</c>).
    /// </returns>
    [HttpGet]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.All)]
    [ProducesResponseType(typeof(IReadOnlyList<ClientUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyUsersAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<ClientUserDto>>>(new GetMyClientUsersQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Invites someone to the authenticated client's own account.</summary>
    /// <param name="command">Who to invite and what they may do.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when sent; 401; 404 with <c>CLIENT_PROFILE_NOT_FOUND</c> or
    /// <c>ACCOUNT_OWNER_NOT_FOUND</c>; 400 with <c>CANNOT_INVITE_ACCOUNT_OWNER</c>; 409 with
    /// <c>USER_ALREADY_LINKED</c> or <c>INVITATION_ALREADY_PENDING</c>;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks full access (<c>All</c>).
    /// </returns>
    [HttpPost("invite")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.All)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InviteMyUserAsync([FromBody] InviteMyUserCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command, ct);
        return HandleResult(outcome);
    }

    /// <summary>Removes someone from the authenticated client's own account.</summary>
    /// <param name="userId">The Identity user id to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when removed; 401; 404 with <c>CLIENT_PROFILE_NOT_FOUND</c>;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks full access (<c>All</c>).
    /// </returns>
    [HttpDelete("{userId}")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.All)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMyUserAsync(string userId, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new RemoveMyUserCommand(userId), ct);
        return HandleResult(outcome);
    }
}
