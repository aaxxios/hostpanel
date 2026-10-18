using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Application.Auth.Commands.DisableTwoFactor;
using Innovayse.Hostpanel.Application.Auth.Commands.EnableTwoFactor;
using Innovayse.Hostpanel.Application.Auth.Commands.StartTwoFactorSetup;
using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Two-factor authentication for the signed-in account.
/// </summary>
/// <remarks>
/// Whose account is always the caller's, taken from the credential. There is deliberately no
/// route that names a user: an endpoint that enrolled or disarmed somebody else's second factor
/// would undo the thing it exists to provide.
///
/// Reading whether it is on has no route here — the client profile already carries the flag,
/// and a second endpoint answering the same question is a second thing to keep true.
/// </remarks>
[Route("api/me/2fa")]
[Authorize(Policy = Policies.Member)]
[EnableRateLimiting(RateLimitPolicies.Auth)]
public sealed class MyTwoFactorController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public MyTwoFactorController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>
    /// Issues a TOTP secret to enrol an authenticator app with.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the secret and the <c>otpauth://</c> URI to render as a QR code; 401 without a
    /// session.
    /// </returns>
    /// <remarks>This does not switch two-factor on; <see cref="EnableAsync"/> does.</remarks>
    [HttpPost("setup")]
    [ProducesResponseType(typeof(TwoFactorSetupDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetupAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<TwoFactorSetupDto>>(new StartTwoFactorSetupCommand(), ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Switches two-factor authentication on.
    /// </summary>
    /// <param name="command">The code the authenticator app currently shows.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 when it is on; 400 when the code did not match; 401.</returns>
    [HttpPost("enable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnableAsync([FromBody] EnableTwoFactorCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command, ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Switches two-factor authentication off.
    /// </summary>
    /// <param name="command">A current code from the authenticator app.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 when it is off; 400 when the code did not match; 401.</returns>
    /// <remarks>
    /// A code is required, not merely a signed-in session — see
    /// <see cref="Innovayse.Hostpanel.Application.Auth.Commands.DisableTwoFactor.DisableTwoFactorHandler"/>.
    /// </remarks>
    [HttpPost("disable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DisableAsync([FromBody] DisableTwoFactorCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command, ct);
        return HandleResult(outcome);
    }
}
