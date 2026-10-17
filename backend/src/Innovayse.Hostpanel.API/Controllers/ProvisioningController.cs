using Innovayse.Hostpanel.Application.Provisioning.Commands.ChangePackage;
using Innovayse.Hostpanel.Application.Provisioning.Commands.ChangePassword;
using Innovayse.Hostpanel.Application.Provisioning.Commands.ProvisionService;
using Innovayse.Hostpanel.Application.Provisioning.Commands.SuspendHostedAccount;
using Innovayse.Hostpanel.Application.Provisioning.Commands.TerminateHostedAccount;
using Innovayse.Hostpanel.Application.Provisioning.Commands.UnsuspendHostedAccount;
using Innovayse.Hostpanel.Application.Provisioning.Queries.GetControlPanelSsoUrl;
using Innovayse.Hostpanel.Application.Provisioning.Queries.GetServiceCredentials;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Staff endpoints for managing a provisioned hosting service on its control-panel provider —
/// provisioning, suspension, termination, credentials and single-sign-on.
/// </summary>
[Route("api/provisioning")]
[Authorize(Policy = Policies.ResellerOrAdmin)]
public sealed class ProvisioningController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public ProvisioningController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Provisions a pending hosting service on the configured provider.</summary>
    /// <param name="id">Client service primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 404 with <c>SERVICE_NOT_FOUND</c> when the id names no service; 503 with
    /// <c>NO_ELIGIBLE_SERVER</c> when no server has room; 502 with
    /// <c>PROVISIONING_UPSTREAM_FAILED</c> when the provider rejects the request; 401; 403.
    /// </returns>
    [HttpPost("{id:int}/provision")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ProvisionAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new ProvisionServiceCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Suspends an active hosting service on the provider.</summary>
    /// <param name="id">Client service primary key; wins over any id in the body.</param>
    /// <param name="command">The reason for the suspension.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 404 with <c>SERVICE_NOT_FOUND</c> when the id names no service; 400 with
    /// <c>SERVICE_NOT_PROVISIONED</c> when the service has never been provisioned; 502 with
    /// <c>SERVICE_SUSPEND_UPSTREAM_FAILED</c> when the provider rejects the request; 401; 403.
    /// </returns>
    [HttpPost("{id:int}/suspend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendAsync(int id, [FromBody] SuspendHostedAccountCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { ServiceId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Unsuspends a suspended hosting service on the provider.</summary>
    /// <param name="id">Client service primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 404 with <c>SERVICE_NOT_FOUND</c> when the id names no service; 400 with
    /// <c>SERVICE_NOT_PROVISIONED</c> when the service has never been provisioned; 502 with
    /// <c>SERVICE_UNSUSPEND_UPSTREAM_FAILED</c> when the provider rejects the request; 401; 403.
    /// </returns>
    [HttpPost("{id:int}/unsuspend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnsuspendAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new UnsuspendHostedAccountCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Permanently terminates a hosting service on the provider.</summary>
    /// <param name="id">Client service primary key; wins over any id in the body.</param>
    /// <param name="command">The reason for the termination.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 404 with <c>SERVICE_NOT_FOUND</c> when the id names no service; 400 with
    /// <c>SERVICE_NOT_PROVISIONED</c> when the service has never been provisioned; 502 with
    /// <c>SERVICE_TERMINATE_UPSTREAM_FAILED</c> when the provider rejects the request; 401; 403.
    /// </returns>
    [HttpPost("{id:int}/terminate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TerminateAsync(int id, [FromBody] TerminateHostedAccountCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { ServiceId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Retrieves the current login credentials for a provisioned hosting service.</summary>
    /// <param name="id">Client service primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the credentials; 404 with <c>SERVICE_NOT_FOUND</c> when the id names no service;
    /// 400 with <c>SERVICE_NOT_PROVISIONED</c> when the service has never been provisioned; 502
    /// with <c>SERVICE_CREDENTIALS_UPSTREAM_FAILED</c> when the provider cannot return them; 401;
    /// 403.
    /// </returns>
    [HttpGet("{id:int}/credentials")]
    [ProducesResponseType(typeof(ServiceCredentialsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCredentialsAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<ServiceCredentialsDto>>(new GetServiceCredentialsQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Changes the hosting account password on the provisioning server.</summary>
    /// <param name="id">Client service primary key; wins over any id in the body.</param>
    /// <param name="command">The new password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 404 with <c>SERVICE_NOT_FOUND</c> when the id names no service; 400 with
    /// <c>SERVICE_NOT_PROVISIONED</c> or <c>SERVICE_NO_SERVER_ASSIGNED</c>; 502 with
    /// <c>SERVICE_PASSWORD_UPSTREAM_FAILED</c> when the provider rejects the change; 401; 403.
    /// </returns>
    [HttpPost("{id:int}/change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePasswordAsync(int id, [FromBody] ChangePasswordCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { ServiceId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Changes the hosting package on the provisioning server.</summary>
    /// <param name="id">Client service primary key; wins over any id in the body.</param>
    /// <param name="command">The new hosting package.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 404 with <c>SERVICE_NOT_FOUND</c> when the id names no service; 400 with
    /// <c>SERVICE_NOT_PROVISIONED</c> or <c>SERVICE_NO_SERVER_ASSIGNED</c>; 502 with
    /// <c>SERVICE_PACKAGE_UPSTREAM_FAILED</c> when the provider rejects the change; 401; 403.
    /// </returns>
    [HttpPost("{id:int}/change-package")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePackageAsync(int id, [FromBody] ChangePackageCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { ServiceId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Generates a single-sign-on URL for direct control panel access.</summary>
    /// <param name="id">Client service primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with <c>{ url }</c> carrying the control-panel SSO address; 404 with
    /// <c>SERVICE_NOT_FOUND</c> when the id names no service; 400 with
    /// <c>SERVICE_NOT_PROVISIONED</c> or <c>SERVICE_NO_SERVER_ASSIGNED</c>; 502 with
    /// <c>SERVICE_CPANEL_SSO_UPSTREAM_FAILED</c> when the provider cannot issue one; 401; 403.
    /// </returns>
    [HttpGet("{id:int}/cpanel-sso")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCPanelSsoUrlAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<string>>(new GetControlPanelSsoUrlQuery(id), ct);
        return HandleResult(outcome, url => HandleData(new { url }));
    }
}
