using Innovayse.Hostpanel.API.Auth;
using Innovayse.Hostpanel.Application.Auth.Commands.ConfirmEmail;
using Innovayse.Hostpanel.Application.Auth.Commands.ForgotPassword;
using Innovayse.Hostpanel.Application.Auth.Commands.Register;
using Innovayse.Hostpanel.Application.Auth.Commands.ResetPassword;
using Innovayse.Hostpanel.Application.Auth.Commands.SignIn;
using Innovayse.Hostpanel.Application.Auth.Commands.SignOut;
using Innovayse.Hostpanel.Application.Auth.Commands.TwoFactorSignIn;
using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Local authentication endpoints. Password sign-in, registration and the password flows
/// that go with it, every one of them meaningless where an external sign-on provider owns the accounts.
/// </summary>
/// <remarks>
/// <para>
/// Every route answers 404 unless the deployment is in local mode, through
/// <see cref="LocalModeOnlyFilter"/>, which runs before the bus is reached. That is also what lets
/// the use cases behind these routes depend on services registered only in local mode: they are
/// never resolved anywhere else.
/// </para>
/// <para>
/// The admin SPA signs in through the shared cookie session under external sign-on mode and only
/// falls back to this form under local mode, which is what <c>GET /api/auth/mode</c> tells it.
/// </para>
/// <para>
/// A sign-in sets the httpOnly session cookie and also returns the token in the body, because the
/// endpoint has a second class of caller, anything scripted or machine-to-machine, which has no
/// cookie jar. See <c>LocalSessionCookie</c> for why this is the same answer the external sign-on
/// path already reached.
/// </para>
/// </remarks>
[Route("api/auth")]
[TypeFilter(typeof(LocalModeOnlyFilter))]
[EnableRateLimiting(RateLimitPolicies.Auth)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
public sealed class LocalAuthController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public LocalAuthController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// Returns an access token, or a pending token if 2FA is required.
    /// </summary>
    /// <param name="command">Email and password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the access token, or with a pending token when a second factor is required; 400 (problem+json) when the email or password is missing; 401 (problem+json) for bad credentials; 404 when local sign-in is not enabled; 423 when the account is locked out after too many wrong passwords; 429 when the rate limit is hit; 500 on an unexpected server fault.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SessionIssuedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TwoFactorPendingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status423Locked)]
    public async Task<IActionResult> LoginAsync([FromBody] SignInCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<ISessionGrant>>(command, ct);
        return HandleSession(outcome);
    }

    /// <summary>
    /// Completes two-factor authentication using a pending token and a TOTP code.
    /// Returns an access token on success.
    /// </summary>
    /// <param name="command">The pending token and the TOTP code.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the access token; 400 when the body fails validation; 401 when the code or pending token is wrong or spent, or the account no longer exists; 404 when local sign-in is not enabled; 429 when the rate limit is hit; 500 on an unexpected server fault.</returns>
    [HttpPost("2fa-login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SessionIssuedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TwoFactorLoginAsync([FromBody] TwoFactorSignInCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<ISessionGrant>>(command, ct);
        return HandleSession(outcome);
    }

    /// <summary>
    /// Ends a local-mode browser session by clearing the httpOnly session cookie.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 once the cookie has been cleared; 404 when local sign-in is not enabled; 429 when the rate limit is hit; 500 on an unexpected server fault.</returns>
    /// <remarks>
    /// <para>
    /// The credential is an HTTP-only cookie: a cookie the page cannot read is also a cookie the
    /// page cannot delete, so signing out has to ask the server.
    /// </para>
    /// <para>
    /// <b>Why <c>DELETE</c> and not <c>POST</c>.</b> Under external sign-on mode <c>MapInnovayseAuth</c> maps
    /// <c>POST /api/auth/logout</c>. A controller action on the same method and path would give
    /// the router two candidate endpoints for one request, an ambiguous match, which fails at
    /// request time rather than at startup, and only on the mode this route is not for. Proven,
    /// not assumed: a throwaway build with <c>[HttpPost("logout")]</c> answered 500
    /// <c>AmbiguousMatchException</c> under <c>Auth__Mode=sso</c>.
    /// </para>
    /// <para>
    /// The verb is what keeps the path free, exactly as it does for login: the SDK serves
    /// <c>GET /api/auth/login</c> (a redirect into the external sign-on provider) while this controller serves
    /// <c>POST /api/auth/login</c>, and the router tells them apart by method alone. Ending a
    /// session is a deletion, so <c>DELETE</c> is both the free verb and the accurate one.
    /// </para>
    /// <para>
    /// Anonymous, and answering 200 either way: clearing a cookie that was not there is not a
    /// failure, and requiring a valid session to end one would leave an operator whose token had
    /// already expired unable to sign out.
    /// </para>
    /// <para>
    /// There is no server-side record of a single local session to revoke: the token is
    /// stateless, and what ties it to the account is the session version it carries. Moving that
    /// version on would sign the person out on every device, which is what a password reset,
    /// switching two-factor off or an address change do, not what signing out of one browser
    /// should do. Clearing the cookie ends this browser's session; a copy of the token taken
    /// elsewhere lives out its short lifetime and is never renewed past the absolute maximum age.
    /// </para>
    /// </remarks>
    [HttpDelete("logout")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SignedOutDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> LocalLogoutAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<SignedOutDto>>(new SignOutCommand(), ct);
        return HandleSessionEnded(outcome);
    }

    /// <summary>
    /// Registers a new user account with email and password.
    /// Returns the newly created user's ID.
    /// </summary>
    /// <param name="command">Email, password and name of the new account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the new user id; 400 (problem+json) when the account cannot be created; 404 when local sign-in is not enabled; 429 when the rate limit is hit; 500 on an unexpected server fault.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisteredUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<RegisteredUserDto>>(command, ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Asks for a password reset link for the given email. The lookup, the token and the mail run in
    /// the background, so the answer is the same, in the same time, whether or not the address
    /// belongs to an account.
    /// </summary>
    /// <param name="command">The email address to send the reset link to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 whether or not the address belongs to an account; 400 when the body fails validation; 404 when local sign-in is not enabled; 429 when the rate limit is hit; 500 on an unexpected server fault.</returns>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command, ct);
        return HandleResultOk(outcome);
    }

    /// <summary>
    /// Resets a user's password using a previously issued reset token.
    /// </summary>
    /// <param name="command">Email, reset token and the new password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 when the password was changed; 400 when the token is invalid or expired; 403 for a published demo account on a demo deployment; 404 when local sign-in is not enabled; 429 when the rate limit is hit; 500 on an unexpected server fault.</returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command, ct);
        return HandleResultOk(outcome);
    }

    /// <summary>
    /// Confirms a user's email address using the token sent during registration.
    /// </summary>
    /// <param name="command">Email and the confirmation token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 when the address is confirmed; 400 when the token is invalid or expired; 404 when local sign-in is not enabled; 429 when the rate limit is hit; 500 on an unexpected server fault.</returns>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command, ct);
        return HandleResultOk(outcome);
    }
}
