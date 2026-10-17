using Innovayse.Hostpanel.API.Auth.Requests;
using Innovayse.Hostpanel.Application.Auth.Commands.ResetPassword;
using Innovayse.Hostpanel.Application.Auth.Commands.SendVerificationEmail;
using Innovayse.Hostpanel.Application.Auth.Commands.SignIn;
using Innovayse.Hostpanel.Application.IntegrationEvents.Contracts;
using Innovayse.Hostpanel.Application.Notifications.Seeding;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Shared.Contracts.Interfaces;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wolverine;

namespace Innovayse.Hostpanel.API.Auth;
/// <summary>
/// Local authentication endpoints. Password sign-in, registration and the password flows
/// that go with it — every one of them meaningless where an SSO owns the accounts.
/// </summary>
/// <remarks>
/// The service that backs these is resolved per action rather than injected, because it is
/// not registered at all in SSO mode. Taking it as a constructor parameter would make every
/// route on this controller fail to resolve there — including the ones that answer 404 on
/// purpose, which would then answer 500 instead.
///
/// <para>
/// An earlier comment here claimed login and 2FA were "always active (needed by admin
/// panel)". They are not: the admin SPA signs in through the shared cookie session under
/// SSO mode and only falls back to this form under local mode, which is what
/// <c>GET /api/auth/mode</c> tells it. Left ungated, these two routes were the reason this
/// controller had to keep a dependency the mode it serves does not have.
/// </para>
/// <para>
/// <b>Deliberately left outside <c>API/Controllers/</c> and on <c>ControllerBase</c>, not
/// <c>BaseApiController</c>.</b> Every action here answers with the framework's own
/// <c>Ok</c>/<c>Unauthorized</c>/<c>NotFound</c>/<c>BadRequest</c> shapes rather than a
/// <see cref="Innovayse.Hostpanel.Shared.Models.Result"/>-mapped one: the token/cookie contract on
/// <c>login</c>, <c>2fa-login</c> and <c>logout</c> is consumed by the SPA's own token-refresh
/// code and by <c>LocalSessionCookie</c>, and rewriting it onto <c>Result</c>/<c>HandleResult</c>
/// would change response shapes a signed-in browser session depends on for a controller task
/// scoped to error contracts and folder layout, not authentication. This is the "framework/identity
/// edge" the conformance plan names explicitly: kept exactly as it behaves today, everywhere else
/// the plan touches. A conversion belongs to its own reviewed change once the SPA's login flow is
/// re-verified end to end, not a side effect of moving files.
/// </para>
/// <para>
/// <b>Its six <c>*Request</c> records — <see cref="Requests.ConfirmEmailRequest"/>,
/// <see cref="Requests.ForgotPasswordRequest"/>, <see cref="Requests.LoginRequest"/>,
/// <see cref="Requests.RegisterRequest"/>, <see cref="Requests.ResetPasswordRequest"/> and
/// <see cref="Requests.TwoFactorLoginRequest"/> — stay, deliberately, for the same reason.</b>
/// The conformance plan's own rule for this controller is: a <c>*Request</c> becomes a command
/// bound directly only where the action already dispatches one use case through
/// <see cref="Wolverine.IMessageBus"/> end to end. None of these six does. <c>LoginAsync</c> and
/// <c>TwoFactorLoginAsync</c> call <see cref="IUserService"/>/<see cref="IJwtService"/> and issue
/// the session cookie directly; <c>ResetPasswordAsync</c> and <c>ConfirmEmailAsync</c> call
/// <see cref="IUserService"/> directly and never touch the bus at all; <c>RegisterAsync</c> and
/// <c>ForgotPasswordAsync</c> call <see cref="IUserService"/> directly and only reach the bus
/// afterward, for one downstream side effect (an integration event, a templated email) that is
/// not the action's own input shape. There is no command here for a body to bind to — inventing
/// one, and the handler behind it, would be the CQRS conversion this remarks section already
/// defers to its own reviewed change, not a side effect of deleting a DTO. The six records stay
/// in <c>API/Auth/Requests/</c>, already the folder next to this controller that
/// <c>hostpanel/rules/project.md</c> sanctions for a request DTO the controller maps onto its own
/// call, exactly where they were before this pass.
/// </para>
/// </remarks>
[ApiController]
[Route("api/auth")]
[EnableRateLimiting(RateLimitPolicies.Auth)]
public sealed class LocalAuthController(
    IServiceProvider services,
    IJwtService tokenService,
    IMessageBus bus,
    IOptions<ClientPortalOptions> clientPortal,
    IAuthModeProvider authMode,
    IErrorMessageResolver errorMessages,
    ILogger<LocalAuthController> logger) : ControllerBase
{
    /// <summary>
    /// Whether this deployment owns its own people. Used to be its own inline, plain
    /// <c>==</c> comparison here — case-sensitive, unlike the equivalent checks elsewhere,
    /// so a configured value of <c>Local</c> made this controller disagree with the rest of
    /// the process about which mode it was in. Now shared through <see cref="IAuthModeProvider"/>.
    /// </summary>
    private bool IsLocalMode => authMode.IsLocalMode;

    /// <summary>
    /// The local user service, resolved only once a route has confirmed the mode.
    /// </summary>
    private IUserService Users => services.GetRequiredService<IUserService>();

    /// <summary>
    /// Answers a refusal in the product's one error shape (problem+json, code in <c>type</c>,
    /// sentence localized by code). This controller is outside <c>BaseApiController</c>, so it
    /// composes the same pieces <c>BaseApiController.MapFailure</c> does instead of naming a
    /// status or building a body of its own.
    /// </summary>
    /// <param name="code">A <see cref="HostpanelErrorCodes"/> constant.</param>
    /// <returns>The response, with the status <see cref="StatusMapping"/> gives the code.</returns>
    private IActionResult Refuse(string code)
    {
        var (status, title) = StatusMapping.For(code);
        return ErrorBody.ToActionResult(new ErrorResponse(code, title, status, errorMessages.Resolve(code)));
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// Returns an access token, or a pending token if 2FA is required.
    /// </summary>
    /// <param name="request">Email and password.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>200 with the access token, or with a pending token when a second factor is required; 400 (problem+json) when the email or password is missing; 401 (problem+json) for bad credentials; 404 when local sign-in is not enabled.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (!IsLocalMode)
        {
            return NotFound();
        }

        // Validation runs on the bus: an omitted email or password is a 400, not a null argument below.
        await bus.InvokeAsync<Result>(new SignInCommand(request.Email, request.Password), ct);

        var userService = Users;
        var user = await userService.FindByEmailAndPasswordAsync(request.Email, request.Password, ct);
        if (user is null)
        {
            return Refuse(HostpanelErrorCodes.SIGN_IN_UNAUTHORIZED);
        }

        // Check 2FA
        var is2FaEnabled = await userService.IsTwoFactorEnabledAsync(user.Value.Id, ct);
        if (is2FaEnabled)
        {
            // Return a pending token (just the user ID encrypted/encoded — simple approach)
            var pendingToken = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"2fa:{user.Value.Id}:{DateTime.UtcNow.Ticks}"));
            return Ok(new { twoFactorRequired = true, pendingToken });
        }

        await userService.UpdateLastLoginAsync(user.Value.Id, ct);
        var roles = await userService.GetRolesAsync(user.Value.Id, ct);
        var emailConfirmed = await userService.IsEmailConfirmedAsync(user.Value.Id, ct);

        var accessToken = tokenService.GenerateAccessToken(
            user.Value.Id, user.Value.Email, null, null, [.. roles], emailConfirmed);

        // The browser's copy goes into an httpOnly cookie it cannot read. The token stays in
        // the body as well, because this endpoint has a second class of caller — anything
        // scripted or machine-to-machine — and taking it out would break them for a browser
        // problem they do not have. See LocalSessionCookie for why this is the same answer the
        // SSO path already reached.
        LocalSessionCookie.Issue(HttpContext, accessToken);

        return Ok(new { accessToken, expiresIn = (int)tokenService.AccessTokenLifetime.TotalSeconds });
    }

    /// <summary>
    /// Completes two-factor authentication using a pending token and a TOTP code.
    /// Returns an access token on success.
    /// </summary>
    /// <param name="request">The pending token and the TOTP code.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>200 with the access token; 401 when the code or pending token is wrong; 404 when local sign-in is not enabled.</returns>
    [HttpPost("2fa-login")]
    [AllowAnonymous]
    public async Task<IActionResult> TwoFactorLoginAsync([FromBody] TwoFactorLoginRequest request, CancellationToken ct)
    {
        if (!IsLocalMode)
        {
            return NotFound();
        }

        // Decode pending token
        string userId;
        try
        {
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(request.PendingToken));
            var parts = decoded.Split(':');
            if (parts.Length < 2 || parts[0] != "2fa")
            {
                return Refuse(HostpanelErrorCodes.PENDING_TOKEN_UNAUTHORIZED);
            }

            userId = parts[1];
        }
        catch
        {
            return Refuse(HostpanelErrorCodes.PENDING_TOKEN_UNAUTHORIZED);
        }

        var userService = Users;
        var valid = await userService.VerifyTwoFactorCodeAsync(userId, request.Code, ct);
        if (!valid)
        {
            return Refuse(HostpanelErrorCodes.TWO_FACTOR_CODE_UNAUTHORIZED);
        }

        await userService.UpdateLastLoginAsync(userId, ct);
        var user = await userService.FindByIdAsync(userId, ct);
        if (user is null)
        {
            return Refuse(HostpanelErrorCodes.SIGN_IN_UNAUTHORIZED);
        }

        var roles = await userService.GetRolesAsync(userId, ct);
        var accessToken = tokenService.GenerateAccessToken(
            userId, user.Value.Email, null, null, [.. roles]);

        // Same as the credential step above: the browser's copy goes into the httpOnly cookie.
        // A sign-in that finished at the second factor has to end in the same session shape as
        // one that did not, or an operator with 2FA enabled would be the only one still holding
        // a token in script-readable storage.
        LocalSessionCookie.Issue(HttpContext, accessToken);

        return Ok(new { accessToken, expiresIn = (int)tokenService.AccessTokenLifetime.TotalSeconds });
    }

    /// <summary>
    /// Ends a local-mode browser session by clearing the httpOnly session cookie.
    /// </summary>
    /// <returns>200 once the cookie has been cleared.</returns>
    /// <remarks>
    /// <para>
    /// It exists because the credential is no longer something the browser can drop on its own.
    /// Signing out used to be <c>sessionStorage.removeItem</c>; a cookie the page cannot read is
    /// also a cookie the page cannot delete, so the server has to be asked.
    /// </para>
    /// <para>
    /// <b>Why <c>DELETE</c> and not <c>POST</c>.</b> Under SSO mode <c>MapInnovayseAuth</c> maps
    /// <c>POST /api/auth/logout</c>. A controller action on the same method and path would give
    /// the router two candidate endpoints for one request — an ambiguous match, which fails at
    /// request time rather than at startup, and only on the mode this route is not for. Proven,
    /// not assumed: a throwaway build with <c>[HttpPost("logout")]</c> answered 500
    /// <c>AmbiguousMatchException</c> under <c>Auth__Mode=sso</c>.
    /// </para>
    /// <para>
    /// The verb is what keeps the path free, exactly as it does for login: the SDK serves
    /// <c>GET /api/auth/login</c> (a redirect into the SSO) while this controller serves
    /// <c>POST /api/auth/login</c>, and the router tells them apart by method alone. Ending a
    /// session is a deletion, so <c>DELETE</c> is both the free verb and the accurate one.
    /// </para>
    /// <para>
    /// Anonymous, and answering 200 either way: clearing a cookie that was not there is not a
    /// failure, and requiring a valid session to end one would leave an operator whose token had
    /// already expired unable to sign out.
    /// </para>
    /// </remarks>
    [HttpDelete("logout")]
    [AllowAnonymous]
    public IActionResult LocalLogout()
    {
        if (!IsLocalMode)
        {
            return NotFound();
        }

        LocalSessionCookie.Clear(HttpContext);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Registers a new user account with email and password.
    /// Returns the newly created user's ID.
    /// </summary>
    /// <param name="request">Email, password and name of the new account.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>200 with the new user id; 400 (problem+json) when the account cannot be created; 404 when local sign-in is not enabled.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request, CancellationToken ct)
    {
        if (!IsLocalMode)
        {
            return NotFound();
        }

        var userService = Users;
        string userId;
        try
        {
            userId = await userService.CreateAsync(
                request.Email, request.Password, request.FirstName, request.LastName, ct);

            // A user row on its own is not an account anyone can use. Both of these went
            // missing when registration was rewritten to call IUserService directly: the
            // RegisterHandler this replaced assigned the role and published the event, and
            // CreateClientOnRegisterHandler — still there, still idempotent — was left
            // waiting for a message nobody sent. What a locally-registered user got was an
            // AppUser with no role and no Client record, which is 403 on every client
            // endpoint and 401 on their own profile. The SSO provisioning path in
            // UserService assigns the same role for the same reason.
            await userService.AddToRoleAsync(userId, Roles.Client, ct);
            await bus.PublishAsync(new ClientRegisteredIntegrationEvent(
                userId, request.Email, request.FirstName ?? string.Empty, request.LastName ?? string.Empty));
        }
        catch (Exception ex)
        {
            // The identity layer's message is an English, internal sentence; it goes to the log
            // and the caller gets the localized one for the code.
            logger.LogWarning(ex, "Local registration was refused.");
            return Refuse(HostpanelErrorCodes.REGISTRATION_FAILED);
        }

        // Outside the try on purpose: the account exists by now, so a mail that cannot be sent
        // must not turn a successful registration into REGISTRATION_FAILED. The user can ask for
        // the link again through POST /api/auth/resend-verification.
        var verification = await bus.InvokeAsync<Result>(new SendVerificationEmailCommand(userId), ct);
        if (verification.IsFailure)
        {
            logger.LogWarning(
                "The verification email for a new account was not sent: {ErrorCode}.", verification.ErrorCode);
        }

        return Ok(new { userId });
    }

    /// <summary>
    /// Initiates a password reset flow by generating a reset token for the given email.
    /// Always returns 200 to avoid user enumeration.
    /// </summary>
    /// <param name="request">The email address to send the reset link to.</param>
    /// <param name="context">Database context the reset is recorded in.</param>
    /// <param name="uow">Unit of work that commits the change.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>200 whether or not the address belongs to an account; 404 when local sign-in is not enabled.</returns>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequest request,
        [FromServices] IHostpanelDbContext context,
        [FromServices] IUnitOfWork uow,
        CancellationToken ct)
    {
        if (!IsLocalMode)
        {
            return NotFound();
        }

        var userService = Users;
        var user = await userService.FindByEmailAsync(request.Email, ct);
        if (user is null)
        {
            return Ok(); // Silent — no enumeration
        }

        var token = await userService.GeneratePasswordResetTokenAsync(user.Value.Id, ct);

        await PasswordResetTemplateSeeder.EnsureSeededAsync(context, uow, ct);

        // Where the portal lives is a bound setting with its own validated default, not a
        // key read by string here — the fallback that used to sit on this line was a second,
        // silent copy of ClientPortalOptions.BaseUrl's own default and could drift from it.
        var clientBaseUrl = clientPortal.Value.BaseUrl;
        var resetLink = $"{clientBaseUrl}/client/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Value.Email)}";

        await bus.InvokeAsync(new SendEmailCommand(
            user.Value.Email,
            PasswordResetTemplateSeeder.Slug,
            new { reset_link = resetLink }), ct);

        return Ok();
    }

    /// <summary>
    /// Resets a user's password using a previously issued reset token.
    /// </summary>
    /// <param name="request">Email, reset token and the new password.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>200 when the password was changed; 400 when the token is invalid or expired; 404 when local sign-in is not enabled.</returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        if (!IsLocalMode)
        {
            return NotFound();
        }

        var outcome = await bus.InvokeAsync<Result>(
            new ResetPasswordCommand(request.Email, request.Token, request.NewPassword), ct);
        return outcome.IsSuccess ? Ok() : Refuse(outcome.ErrorCode!);
    }

    /// <summary>
    /// Confirms a user's email address using the token sent during registration.
    /// </summary>
    /// <param name="request">Email and the confirmation token.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>200 when the address is confirmed; 400 when the token is invalid or expired; 404 when local sign-in is not enabled.</returns>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailRequest request, CancellationToken ct)
    {
        if (!IsLocalMode)
        {
            return NotFound();
        }

        var success = await Users.ConfirmEmailAsync(request.Email, request.Token, ct);
        return success ? Ok() : Refuse(HostpanelErrorCodes.TOKEN_INVALID_OR_EXPIRED);
    }

    /// <summary>
    /// Refresh token endpoint — not implemented; short-lived tokens require re-login when expired.
    /// </summary>
    /// <returns>Always 401: the session has to be re-established by signing in again.</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public IActionResult RefreshAsync()
    {
        // Refresh token handling would need a token store — out of scope for now
        // The BFF sets short-lived access tokens; the user re-logs when expired
        return Refuse(HostpanelErrorCodes.SESSION_UNAUTHORIZED);
    }
}
