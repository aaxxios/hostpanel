using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Queries.GetTldPricing;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Public domain lookup endpoints. Domain availability checking lives in
/// <see cref="Innovayse.Hostpanel.API.Controllers.DomainsController"/>.
/// </summary>
[Route("api/domains")]
public sealed class DomainLookupController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public DomainLookupController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns TLD pricing information for domain registration, transfer, and renewal.</summary>
    /// <param name="currency">Target currency code (e.g. "USD", "RUB", "AMD"). Defaults to USD.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with TLD pricing data and per-extension price breakdowns. Anonymous: the storefront and
    /// client area both need it before or without a session.
    /// </returns>
    [HttpGet("tld-pricing")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Upstream)]
    [ProducesResponseType(typeof(TldPricingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTldPricingAsync([FromQuery] string? currency, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<TldPricingDto>>(new GetTldPricingQuery(currency), ct);
        return HandleResult(outcome);
    }
}
