using Innovayse.Hostpanel.API.Authorization;
using Innovayse.Hostpanel.Application.Auth.Commands.BackchannelLogout;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Server-to-server endpoint the platform portal calls when a person signs out, so the sessions
/// this product holds for them end immediately.
/// </summary>
[Route("auth")]
[AllowAnonymous]
[BackchannelKey]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
public sealed class BackchannelLogoutController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public BackchannelLogoutController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Ends every session held for the person named by the body.</summary>
    /// <remarks>
    /// Authenticated by the shared key in <c>X-Logout-Key</c>, not by a session: the caller is the
    /// portal acting for someone who has already signed out. Idempotent: an unknown address answers 200.
    /// </remarks>
    /// <param name="command">The person who signed out.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 once the sessions are gone; 400 when the body fails validation, such as a malformed address; 401 for a missing or wrong key; 429 when the rate limit is hit.</returns>
    [HttpPost("backchannel-logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BackchannelLogoutAsync([FromBody] BackchannelLogoutCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command, ct);
        return HandleResultOk(outcome);
    }
}
