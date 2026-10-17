using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
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
public sealed class ReferenceDataController : BaseApiController
{
    /// <summary>Returns the full list of ISO 3166-1 alpha-2 countries.</summary>
    /// <returns>200 with the country list; 401.</returns>
    [HttpGet("countries")]
    [ProducesResponseType(typeof(IReadOnlyList<CountryDto>), StatusCodes.Status200OK)]
    public IActionResult GetCountries() => HandleData(CountryList.All);

    /// <summary>Returns the list of supported currencies.</summary>
    /// <returns>200 with the currency list.</returns>
    [AllowAnonymous]
    [HttpGet("currencies")]
    [ProducesResponseType(typeof(IReadOnlyList<CurrencyOptionDto>), StatusCodes.Status200OK)]
    public IActionResult GetCurrencies() => HandleData(CurrencyList.All);
}
