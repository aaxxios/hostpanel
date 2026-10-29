using Innovayse.Hostpanel.Application.Billing.Currencies.Commands.SetBaseCurrency;
using Innovayse.Hostpanel.Application.Billing.Currencies.Commands.UpdateExchangeRates;
using Innovayse.Hostpanel.Application.Billing.Currencies.Commands.UpsertCurrency;
using Innovayse.Hostpanel.Application.Billing.Currencies.Common;
using Innovayse.Hostpanel.Application.Billing.Currencies.Queries.ListCurrencies;
using Innovayse.Hostpanel.Application.Billing.Currencies.Queries.ListPublicCurrencies;
using Innovayse.Hostpanel.Application.Products.Commands.UpdateProductPrices;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>The currencies this deployment prices in, and the rates between them.</summary>
[Route("api/admin/currencies")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
public sealed class CurrenciesController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public CurrenciesController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Every configured currency, the base one first.</summary>
    /// <param name="query">Whether to leave out the currencies the operator switched off.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the currencies; 401 without a session; 403 for a caller who is not staff; 400 when the body, query or route fails validation; 429 when the rate limit is hit.</returns>
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(IReadOnlyList<CurrencyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync([FromQuery] ListCurrenciesQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<CurrencyDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a currency, or rewrites the one already under that code.</summary>
    /// <param name="code">ISO 4217 alpha code; wins over any code in the body.</param>
    /// <param name="command">The currency to write.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 when written; 400 when a field is refused; 401 without a session; 403 for a caller who is not an admin; 429 when the rate limit is hit.</returns>
    [HttpPut("{code}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpsertAsync(
        string code, [FromBody] UpsertCurrencyCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Code = code }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Makes a currency the base, re-expressing every other rate against it.</summary>
    /// <param name="code">ISO 4217 alpha code of the currency to promote.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 when promoted; 404 when the code names no configured currency; 401 without a session; 403 for a caller who is not an admin; 429 when the rate limit is hit.</returns>
    [HttpPost("{code}/make-base")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MakeBaseAsync(string code, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new SetBaseCurrencyCommand(code), ct);
        return HandleResult(outcome);
    }

    /// <summary>Refreshes every non-base rate from the configured rate source.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with what was updated and what the source did not quote; 503 (with <c>Retry-After</c>) when
    /// no base currency is configured or the rate source is unavailable; 409 when the source does not
    /// quote the base currency; 401 without a session; 403 for a caller who is not an admin;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpPost("update-rates")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(UpdateExchangeRatesResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdateRatesAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<UpdateExchangeRatesResultDto>>(
            new UpdateExchangeRatesCommand(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Rewrites product prices in the named currencies from their base-currency prices.</summary>
    /// <param name="command">The currencies to recompute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with how many price rows were written; 400 when the base itself was named; 404 when a code
    /// names no configured currency; 503 when no base currency is configured; 401 without a session; 403 for a caller who is not an admin;
    /// 429 when the rate limit is hit.
    /// </returns>
    [HttpPost("update-product-prices")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdateProductPricesAsync(
        [FromBody] UpdateProductPricesCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// The currencies a visitor may price in. Anonymous: the storefront needs it before anyone has
    /// signed in. Rates are not in the answer.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the enabled currencies; 429 when the rate limit is hit.</returns>
    [HttpGet("/api/currencies")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<PublicCurrencyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPublicAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<PublicCurrencyDto>>>(
            new ListPublicCurrenciesQuery(), ct);
        return HandleResult(outcome);
    }
}
