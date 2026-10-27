using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Domains.Commands.CancelTransfer;
using Innovayse.Hostpanel.Application.Domains.Commands.InitiateOutgoingTransfer;
using Innovayse.Hostpanel.Application.Domains.Commands.RegisterDomain;
using Innovayse.Hostpanel.Application.Domains.Commands.RenewDomain;
using Innovayse.Hostpanel.Application.Domains.Commands.SetAutoRenew;
using Innovayse.Hostpanel.Application.Domains.Commands.TransferDomain;
using Innovayse.Hostpanel.Application.Domains.Commands.UpdateDomain;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Queries.CheckDomainAvailability;
using Innovayse.Hostpanel.Application.Domains.Queries.GetDomain;
using Innovayse.Hostpanel.Application.Domains.Queries.GetWhois;
using Innovayse.Hostpanel.Application.Domains.Queries.ListDomains;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Admin/reseller management of registered domains: listing, registration, transfer, renewal and
/// WHOIS lookup. DNS records, email forwarding, TLD configuration and the client-facing <c>My*</c>
/// domain surface live in their own controllers.
/// </summary>
[Route("api/domains")]
public sealed class DomainsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public DomainsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns a paginated list of all domains in the system.</summary>
    /// <param name="query">Pagination and optional client filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page; 401; 403 for a caller who is neither reseller nor staff.</returns>
    [HttpGet]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(typeof(PagedResult<DomainRegistrationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListDomainsAsync([FromQuery] ListDomainsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PagedResult<DomainRegistrationDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a single domain by its primary key.</summary>
    /// <param name="id">Domain primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the domain; 404 when the id names no domain; 401; 403.</returns>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(typeof(DomainDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<DomainDto>>(new GetDomainQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Performs a WHOIS lookup for a domain name.</summary>
    /// <param name="name">The fully-qualified domain name to look up.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the WHOIS record; 404 when the registrar has no record for the name; 401; 403.
    /// </returns>
    [HttpGet("whois")]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [EnableRateLimiting(RateLimitPolicies.Upstream)]
    [ProducesResponseType(typeof(WhoisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWhoisAsync([FromQuery] string name, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<WhoisDto>>(new GetWhoisQuery(name), ct);
        return HandleResult(outcome);
    }

    /// <summary>Registers a new domain name for a client.</summary>
    /// <param name="command">Registration details including domain name, years and pricing.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 201 with a link to the new domain; 409 when it is already registered through this system;
    /// 502 when the registrar rejects the request; 401; 403.
    /// </returns>
    [HttpPost("register")]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterDomainCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"/api/domains/{id}", (object?)null));
    }

    /// <summary>Initiates an incoming domain transfer for a client.</summary>
    /// <param name="command">Transfer details including domain name, EPP code and pricing.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 201 with a link to the new domain; 502 when the registrar rejects the transfer; 401; 403.
    /// </returns>
    [HttpPost("transfer")]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> TransferAsync([FromBody] TransferDomainCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"/api/domains/{id}", (object?)null));
    }

    /// <summary>Renews a domain registration for additional years.</summary>
    /// <param name="id">Domain primary key; wins over any id in the body.</param>
    /// <param name="command">Renewal details specifying the number of years.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 when the domain has no registrar reference; 404 when the id names no
    /// domain; 502 when the registrar rejects the renewal; 401; 403.
    /// </returns>
    [HttpPost("{id:int}/renew")]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> RenewAsync(int id, [FromBody] RenewDomainCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { DomainId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Enables or disables automatic renewal for a domain.</summary>
    /// <param name="id">Domain primary key; wins over any id in the body.</param>
    /// <param name="command">The desired auto-renew flag value.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 404 when the id names no domain; 502 when the registrar rejects the
    /// update; 401; 403.
    /// </returns>
    [HttpPut("{id:int}/auto-renew")]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> SetAutoRenewAsync(
        int id, [FromBody] SetAutoRenewCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { DomainId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Cancels a pending incoming domain transfer.</summary>
    /// <param name="id">Domain primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 404 when the id names no domain; 502 when the registrar rejects the
    /// cancellation; 401; 403.
    /// </returns>
    [HttpPost("{id:int}/cancel-transfer")]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> CancelTransferAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new CancelTransferCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Initiates an outgoing domain transfer and retrieves the EPP code.</summary>
    /// <param name="id">Domain primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the EPP authorization code; 404 when the id names no domain; 400 when the domain
    /// carries no registrar reference; 502 when the registrar rejects the request or returns no
    /// code; 401; 403.
    /// </returns>
    [HttpPost("{id:int}/initiate-outgoing-transfer")]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> InitiateOutgoingTransferAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<string>>(new InitiateOutgoingTransferCommand(id), ct);
        return HandleResult(outcome, eppCode => HandleData(new { eppCode }));
    }

    /// <summary>Updates editable domain fields.</summary>
    /// <param name="id">Domain primary key; wins over any id in the body.</param>
    /// <param name="command">Updated domain field values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no domain; 401; 403.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateDomainCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { DomainId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Checks whether a domain name is available for registration (GET).</summary>
    /// <param name="name">The fully-qualified domain name to check (e.g. "example.com").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the availability flag. Anonymous: the storefront needs it before sign-in.</returns>
    [HttpGet("check")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Upstream)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckAvailabilityAsync([FromQuery] string name, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<bool>>(new CheckDomainAvailabilityQuery(name), ct);
        return HandleResult(outcome);
    }

    /// <summary>Checks whether a domain name is available for registration (POST).</summary>
    /// <param name="query">Request body containing the domain name to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the name, availability flag and status label; 400 <c>VALIDATION_ERROR</c> when no name was given.
    /// Anonymous: the storefront needs it before sign-in.
    /// </returns>
    [HttpPost("check")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Upstream)]
    [ProducesResponseType(typeof(DomainCheckResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckAvailabilityPostAsync(
        [FromBody] CheckDomainAvailabilityQuery query, CancellationToken ct)
    {
        // A missing or blank name is refused by CheckDomainAvailabilityValidator (400 VALIDATION_ERROR,
        // sentence in the request's language); the controller authors no sentence of its own.
        var domain = query.Domain?.Trim() ?? string.Empty;

        var outcome = await _bus.InvokeAsync<Result<bool>>(new CheckDomainAvailabilityQuery(domain), ct);
        return HandleResult(outcome, available =>
            HandleData(new DomainCheckResultDto(domain, available, available ? "available" : "taken")));
    }
}
