using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Application.Admin.Users.Commands.DeleteUser;
using Innovayse.Hostpanel.Application.Admin.Users.Commands.SendPasswordReset;
using Innovayse.Hostpanel.Application.Admin.Users.Commands.SetUserPassword;
using Innovayse.Hostpanel.Application.Admin.Users.Commands.UpdateUser;
using Innovayse.Hostpanel.Application.Admin.Users.Queries.GetUser;
using Innovayse.Hostpanel.Application.Admin.Users.Queries.ListUsers;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Admin endpoints for managing people.
/// </summary>
/// <remarks>
/// Reads work in both modes. The writes below — editing a profile, changing an address,
/// deleting an account, setting or resetting a password — are only this product's to make
/// where it owns the accounts; where an SSO owns them, they are made there, and each answers
/// its own <c>USER_PROVISIONING_NOT_ALLOWED_*</c> code naming the flow that was refused.
/// </remarks>
[Route("api/admin/users")]
[Authorize(Policy = Policies.Admin)]
public sealed class AdminUsersController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public AdminUsersController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns a paginated list of all users.</summary>
    /// <param name="query">Search term and paging.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page of user summaries; 401; 403.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync([FromQuery] ListUsersQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PagedResult<UserListItemDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a single user with linked client accounts.</summary>
    /// <param name="id">The person's subject.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the user detail; 404 with <c>IDENTITY_USER_NOT_FOUND</c>; 401; 403.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(string id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<UserDetailDto>>(new GetUserQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Updates a user's profile fields.</summary>
    /// <param name="id">The person's subject; wins over any id in the body.</param>
    /// <param name="command">Updated profile data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 with <c>USER_PROVISIONING_NOT_ALLOWED_CHANGE_NAME</c> or
    /// <c>USER_PROVISIONING_NOT_ALLOWED_CHANGE_EMAIL</c> where an SSO owns the accounts; 401; 403.
    /// </returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAsync(string id, [FromBody] UpdateUserCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Deletes an account. Client records that reference it are preserved as orphans.
    /// </summary>
    /// <param name="id">The person's subject.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 with <c>USER_PROVISIONING_NOT_ALLOWED_DELETE_ACCOUNT</c> where an SSO
    /// owns the accounts; 401; 403.
    /// </returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteAsync(string id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteUserCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Sends the user a link they can use to choose a new password.
    /// Seeds the email template on first use.
    /// </summary>
    /// <param name="id">The person's subject.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 404 with <c>IDENTITY_USER_NOT_FOUND</c>; 400 with
    /// <c>USER_PROVISIONING_NOT_ALLOWED_ISSUE_PASSWORD_RESET</c> where an SSO owns the accounts;
    /// 401; 403.
    /// </returns>
    [HttpPost("{id}/reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPasswordAsync(string id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new SendPasswordResetCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Sets a new password for a user directly (admin action).
    /// </summary>
    /// <param name="id">The person's subject; wins over any id in the body.</param>
    /// <param name="command">The new password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 404 with <c>IDENTITY_USER_NOT_FOUND</c>; 400 with
    /// <c>USER_PROVISIONING_NOT_ALLOWED_SET_PASSWORD</c> where an SSO owns the accounts; 401; 403.
    /// </returns>
    [HttpPost("{id}/change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePasswordAsync(
        string id, [FromBody] SetUserPasswordCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }
}
