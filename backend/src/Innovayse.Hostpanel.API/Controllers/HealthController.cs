using Innovayse.Hostpanel.Shared.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Innovayse.Hostpanel.API.Controllers;
/// <summary>
/// Liveness probe endpoint used by load balancers and integration tests
/// to verify the API process is running.
/// </summary>
[AllowAnonymous]
[Route("api/health")]
public sealed class HealthController : BaseApiController
{
    /// <summary>Returns 200 OK when the API process is alive.</summary>
    /// <returns>A JSON object containing <c>status</c> and <c>timestamp</c>.</returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok", timestamp = DateTimeOffset.UtcNow });
}
