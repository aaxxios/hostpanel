using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// The one liveness probe, used by the container healthcheck, the installer and the integration
/// tests to verify the API process is serving HTTP.
/// </summary>
/// <remarks>
/// Deliberately not a database probe: it answers "is the process still serving HTTP", which is the
/// question a restart policy can act on. A probe that also failed on a slow database would restart a
/// healthy process in a loop while the real fault sat elsewhere.
/// <para>
/// Exempt from rate limiting through <see cref="DisableRateLimitingAttribute"/>, the one place an
/// exemption is declared: a probe runs every few seconds forever from the same address, and one that
/// is refused is read by the restart policy as a dead process.
/// </para>
/// </remarks>
[AllowAnonymous]
[Route("health")]
public sealed class HealthController : BaseApiController
{
    /// <summary>Answers 200 while the API process is alive.</summary>
    /// <param name="clock">The source of the moment the process answered at.</param>
    /// <returns>200 with the status and the time the process answered.</returns>
    [HttpGet]
    [DisableRateLimiting]
    [ProducesResponseType(typeof(HealthDto), StatusCodes.Status200OK)]
    public IActionResult Get([FromServices] IClock clock)
    {
        return HandleData(new HealthDto(HealthStatus.Ok, clock.UtcNow));
    }
}
