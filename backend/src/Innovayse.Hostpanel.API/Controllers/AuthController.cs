using Innovayse.Hostpanel.Application.Auth.Commands.ClaimSetup;
using Innovayse.Hostpanel.Application.Auth.Commands.ResendVerification;
using Innovayse.Hostpanel.Application.Auth.Queries.GetAuthMode;
using Innovayse.Hostpanel.Application.Auth.Queries.GetEmailVerified;
using Innovayse.Hostpanel.Application.Auth.Queries.GetMyAccount;
using Innovayse.Hostpanel.Application.Auth.Queries.GetSetupRequired;
using Innovayse.Hostpanel.Application.Clients.Commands.AcceptInvitation;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Auth endpoints remaining after SSO migration; login, register, refresh and logout are the Nuxt BFF's, via SSO directly.</summary>
[Route("api/auth")]
public sealed class AuthController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public AuthController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>
    /// Returns how this deployment signs people in, so a browser client can offer the right
    /// control instead of guessing.
    /// </summary>
    /// <remarks>
    /// Anonymous on purpose: a caller has to read this before it can authenticate, and the answer
    /// is a deployment shape rather than a secret.
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with <c>{ mode }</c>, either <c>local</c> or <c>sso</c>.</returns>
    [HttpGet("mode")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthModeDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ModeAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<AuthModeDto>>(new GetAuthModeQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns whether initial admin setup is required (nobody holds Admin yet).</summary>
    /// <remarks>
    /// Also reports whether a setup <b>token</b> is needed: under <c>local</c> the claim is gated
    /// on the token this installation printed to its log, under <c>sso</c> it is not gated at all.
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with <c>{ required, tokenRequired }</c>.</returns>
    [HttpGet("setup-required")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SetupRequiredDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetupRequiredAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<SetupRequiredDto>>(new GetSetupRequiredQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Initial setup: grants the Admin role to the authenticated caller who presents this
    /// installation's setup token. Only works while nobody holds Admin.
    /// </summary>
    /// <param name="command">The claim, carrying the setup token and nothing else.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with <c>{ success = true }</c> once the role has been granted; 401 with no subject; 403
    /// with <c>SETUP_TOKEN_INVALID</c>; 409 with <c>SETUP_ALREADY_COMPLETED</c>.
    /// </returns>
    [HttpPost("setup")]
    [Authorize(Policy = Policies.Member)]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [ProducesResponseType(typeof(SetupClaimedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetupAsync([FromBody] ClaimSetupCommand? command, CancellationToken ct)
    {
        // The body is optional so that an SSO caller -- where no token exists and the SPA sends
        // none -- still reaches the handler instead of being refused by model binding.
        var outcome = await _bus.InvokeAsync<Result<SetupClaimedDto>>(command ?? new ClaimSetupCommand(null), ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns whether the current user's email is confirmed.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with <c>{ verified }</c>, where <c>verified</c> is <c>null</c> when the credential
    /// carries no <c>email_verified</c> claim; 401.
    /// </returns>
    /// <remarks>
    /// Three-valued on purpose, and clients must keep it that way: prompting on anything other
    /// than an explicit <c>false</c> nags every caller whose token simply lacks the claim.
    /// </remarks>
    [HttpGet("email-verified")]
    [Authorize(Policy = Policies.Member)]
    [ProducesResponseType(typeof(EmailVerifiedDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> EmailVerifiedAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<EmailVerifiedDto>>(new GetEmailVerifiedQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Sends the caller the email-verification mail again. Does nothing when the address is
    /// already confirmed.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 once the mail is sent, or at once when the address is already confirmed; 400 with
    /// <c>USER_PROVISIONING_NOT_ALLOWED_ISSUE_EMAIL_CONFIRMATION</c> where an SSO owns the accounts;
    /// 401; 429 past the rate limit.
    /// </returns>
    [HttpPost("resend-verification")]
    [Authorize(Policy = Policies.Member)]
    [EnableRateLimiting(RateLimitPolicies.Strict)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResendVerificationAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new ResendVerificationCommand(), ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Returns the current SSO-authenticated user's email and local roles, for SPA clients that
    /// cannot decode roles from the SSO-issued token itself.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with <c>{ email, roles, emailVerified }</c>; 401 when no account answers the credential.</returns>
    [HttpGet("me")]
    [Authorize(Policy = Policies.Member)]
    [ProducesResponseType(typeof(MyAccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MeAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<MyAccountDto>>(new GetMyAccountQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Accepts an invitation. The invitation token is validated and the current SSO user is
    /// assigned the role defined in the invitation.
    /// </summary>
    /// <param name="command">The claim, carrying the invitation token and nothing else.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with <c>{ success = true }</c>; 404 with <c>INVITATION_NOT_FOUND</c> or
    /// <c>CLIENT_NOT_FOUND</c>; 400 with <c>INVITATION_EXPIRED</c>; 409 with
    /// <c>INVITATION_ALREADY_ACCEPTED</c>; 401.
    /// </returns>
    [HttpPost("accept-invite")]
    [Authorize(Policy = Policies.Member)]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptInviteAsync([FromBody] AcceptInvitationCommand command, CancellationToken ct)
    {
        // No subject travels on the command: the handler asks the credential itself, so a valid
        // invitation token cannot be redeemed on behalf of a different account.
        var outcome = await _bus.InvokeAsync<Result<string>>(command, ct);
        return HandleResult(outcome, _ => HandleData(new { success = true }));
    }
}
