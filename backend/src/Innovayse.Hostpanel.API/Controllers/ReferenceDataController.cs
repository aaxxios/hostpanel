using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Reference data endpoints for countries and currencies. Not a use case: the lists are
/// hardcoded, so <see cref="BaseApiController.HandleData{T}"/> is the sanctioned way to answer
/// 200 with data that never went through a <see cref="Innovayse.Hostpanel.Shared.Models.Result"/>.
/// </summary>
[Route("api/reference")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
public sealed class ReferenceDataController : BaseApiController
{
    /// <summary>Returns the full list of ISO 3166-1 alpha-2 countries.</summary>
    /// <returns>200 with the country list; 401 without a session; 429 when the rate limit is hit.</returns>
    [HttpGet("countries")]
    [ProducesResponseType(typeof(IReadOnlyList<CountryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult GetCountries() => HandleData(CountryList.All);

    /// <summary>Returns the list of supported currencies.</summary>
    /// <returns>200 with the currency list; 429 when the rate limit is hit.</returns>
    [AllowAnonymous]
    [HttpGet("currencies")]
    [ProducesResponseType(typeof(IReadOnlyList<CurrencyOptionDto>), StatusCodes.Status200OK)]
    public IActionResult GetCurrencies() => HandleData(CurrencyList.All);
}
