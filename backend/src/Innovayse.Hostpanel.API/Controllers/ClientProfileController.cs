using Innovayse.Hostpanel.API.Authorization;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Application.Clients.Commands.AddMyContact;
using Innovayse.Hostpanel.Application.Clients.Commands.RemoveMyContact;
using Innovayse.Hostpanel.Application.Clients.Commands.UpdateMyContact;
using Innovayse.Hostpanel.Application.Clients.Commands.UpdateMyProfile;
using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Queries.GetMyContacts;
using Innovayse.Hostpanel.Application.Clients.Queries.GetMyProfile;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Client self-service portal endpoints: viewing and updating the caller's own profile.</summary>
/// <remarks>
/// Which account is never in the route or the body, and is not read from a claim here either:
/// every use case below resolves the caller inside its own handler, so each action binds and
/// dispatches exactly once.
/// </remarks>
[Route("api/clients/me")]
public sealed class ClientProfileController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public ClientProfileController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns the authenticated client's own profile.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the profile; 401; 404 with <c>CLIENT_PROFILE_NOT_FOUND</c>.</returns>
    [HttpGet]
    [Authorize(Policy = Policies.Client)]
    [AvailableToEveryClientUser]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfileAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<ClientDto>>(new GetMyProfileQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Updates the authenticated client's own profile and billing address.</summary>
    /// <param name="command">The updated profile data. A client id or currency in the body, if any, is ignored.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the updated profile; 401; 404 with <c>CLIENT_PROFILE_NOT_FOUND</c>;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>ModifyMasterProfile</c>.
    /// </returns>
    [HttpPut]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.ModifyMasterProfile)]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyProfileAsync([FromBody] UpdateMyProfileCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<ClientDto>>(command, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns the additional contacts on the authenticated client's own account.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the account's contacts; 401; 404 with <c>CLIENT_PROFILE_NOT_FOUND</c>;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>ViewManageContacts</c>.
    /// </returns>
    [HttpGet("contacts")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.ViewManageContacts)]
    [ProducesResponseType(typeof(IReadOnlyList<ContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyContactsAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<ContactDto>>>(new GetMyContactsQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Adds a contact to the authenticated client's own account.</summary>
    /// <param name="command">The contact to add. A client id in the body, if any, is ignored.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when written; 401; 404 with <c>CLIENT_PROFILE_NOT_FOUND</c>;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>ViewManageContacts</c>.
    /// </returns>
    [HttpPost("contacts")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.ViewManageContacts)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMyContactAsync([FromBody] AddMyContactCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command, ct);
        return HandleResult(outcome);
    }

    /// <summary>Updates a contact on the authenticated client's own account.</summary>
    /// <param name="contactId">The contact primary key; wins over any contact id in the body.</param>
    /// <param name="command">The updated contact data. A client id in the body, if any, is ignored.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when written; 401; 404 with <c>CLIENT_PROFILE_NOT_FOUND</c> or <c>MY_CONTACT_NOT_FOUND</c>;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>ViewManageContacts</c>.
    /// </returns>
    [HttpPut("contacts/{contactId:int}")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.ViewManageContacts)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyContactAsync(
        int contactId, [FromBody] UpdateMyContactCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { ContactId = contactId }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Removes a contact from the authenticated client's own account.</summary>
    /// <param name="contactId">The contact primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when removed; 401; 404 with <c>CLIENT_PROFILE_NOT_FOUND</c> or <c>MY_CONTACT_NOT_FOUND</c>;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>ViewManageContacts</c>.
    /// </returns>
    [HttpDelete("contacts/{contactId:int}")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.ViewManageContacts)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMyContactAsync(int contactId, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new RemoveMyContactCommand(contactId), ct);
        return HandleResult(outcome);
    }
}
