using Innovayse.Hostpanel.Application.Demo.Queries.GetDemo;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// The public demo's front door, readable without a login: whether this deployment is a demo,
/// when its data is next reset, and the accounts a visitor may sign in with. Separate from every
/// admin controller so the anonymous path can never widen by someone adding a field elsewhere.
/// </summary>
[Route("api/public/demo")]
public sealed class DemoController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public DemoController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns the demo status and the published accounts.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with <c>{ enabled, nextResetAt, accounts: [{ role, email, password }] }</c>; on a deployment
    /// that is not a demo, <c>{ enabled: false, nextResetAt: null, accounts: [] }</c>; 429 only when the
    /// global budget is exceeded. No named policy: every page load reads this, so it takes the
    /// same global budget as the other public reads.
    /// </returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DemoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<DemoDto>>(new GetDemoQuery(), ct);
        return HandleResult(outcome);
    }
}
