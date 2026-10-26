using Innovayse.Hostpanel.API.Authorization;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Application.Billing.Queries.GetMyServiceInvoices;
using Innovayse.Hostpanel.Application.Provisioning.Commands.ChangeMyServicePassword;
using Innovayse.Hostpanel.Application.Provisioning.Queries.GetMyServiceControlPanelSsoUrl;
using Innovayse.Hostpanel.Application.Services.Commands.CancelMyService;
using Innovayse.Hostpanel.Application.Services.Commands.OrderMyService;
using Innovayse.Hostpanel.Application.Services.Commands.SetupMyService;
using Innovayse.Hostpanel.Application.Services.Common;
using Innovayse.Hostpanel.Application.Services.Queries.GetMyServiceCancellationStatus;
using Innovayse.Hostpanel.Application.Services.Queries.GetMyServices;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Client portal endpoints for viewing and ordering services.
/// </summary>
/// <remarks>
/// <para>
/// No action here checks ownership, and none may: the rule lives in the client-facing
/// <c>My*</c> handlers, which resolve the caller from the credential themselves. That is not
/// only the thin-controller rule — two of the use cases behind these routes
/// (<c>GetControlPanelSsoUrlQuery</c>, <c>ChangePasswordCommand</c>) are also dispatched by the admin
/// <c>ProvisioningController</c>, where acting on any client's service is legitimate, so a check
/// written here would have guaranteed nothing about the message itself and a check written on
/// the shared use case would have broken the admin path.
/// </para>
/// <para>
/// Every action below that takes a service id off the route dispatches a message implementing
/// <see cref="Innovayse.Hostpanel.Application.Common.Interfaces.ICallerScopedServiceMessage"/>, and a
/// reflection test enforces that of any action added later. A service belonging to somebody
/// else, a service that does not exist, and a caller with no client record all answer 404 with
/// <c>SERVICE_NOT_FOUND</c> and the same sentence; service ids are sequential, and telling
/// those three apart is itself a way of enumerating them.
/// </para>
/// </remarks>
[Route("api/me/services")]
public sealed class MyServicesController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public MyServicesController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns all services belonging to the authenticated client.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the caller's services; 401 without a session; 404 with
    /// <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller has no client record;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>ViewProductsServices</c>.
    /// </returns>
    [HttpGet]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.ViewProductsServices)]
    [ProducesResponseType(typeof(IReadOnlyList<ClientServiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMineAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<ClientServiceDto>>>(new GetMyServicesQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Orders a new service for the authenticated client.</summary>
    /// <param name="command">
    /// The product and billing cycle to order. Amounts are never taken from the request.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 201 with the new service id; 401; 404 with <c>CLIENT_PROFILE_NOT_FOUND</c>,
    /// <c>PRODUCT_NOT_FOUND</c> or <c>PRODUCT_NOT_AVAILABLE</c>;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>PlaceNewOrders</c>.
    /// </returns>
    [HttpPost]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.PlaceNewOrders)]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> OrderAsync([FromBody] OrderMyServiceCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"/api/me/services/{id}", id));
    }

    /// <summary>
    /// Generates a time-limited control panel SSO URL for a service owned by the authenticated client.
    /// </summary>
    /// <param name="id">Client service primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with <c>{ url }</c> carrying the control-panel SSO address; 404 with
    /// <c>SERVICE_NOT_FOUND</c> when the service is not the caller's; 401;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>AllowSingleSignOn</c>.
    /// </returns>
    [HttpGet("{id:int}/cpanel-sso")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.AllowSingleSignOn)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCPanelSsoUrlAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<string>>(new GetMyServiceControlPanelSsoUrlQuery(id), ct);

        // `{ url }`, matching the staff route in ProvisioningController. This answered with a
        // bare JSON string while its sibling answered with an object, and the portal — written
        // against the sibling — destructured `url` off a string, got undefined, and opened a
        // blank tab. Two shapes for one answer is what made that possible.
        return HandleResult(outcome, url => HandleData(new { url }));
    }

    /// <summary>Submits a cancellation request for a client service.</summary>
    /// <param name="id">Client service primary key; wins over any id in the body.</param>
    /// <param name="command">The cancellation type and an optional reason.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 on success; 404 with <c>SERVICE_NOT_FOUND</c> when the service is not the caller's,
    /// or <c>CANCELLATION_ALREADY_PENDING</c> (409) when one is already open; 401;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>PlaceNewOrders</c>.
    /// </returns>
    [HttpPost("{id:int}/cancel")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.PlaceNewOrders)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelAsync(int id, [FromBody] CancelMyServiceCommand command, CancellationToken ct)
    {
        // A `with` overlay compiles to a call into the record's generated Clone(), so the actual
        // constructor call an id-tampering check would need to see never appears in this method's
        // own IL -- MyServicesRoutesAreOwnershipScopedTests scans for it there. Reconstructing
        // explicitly keeps that reflection test able to see every route it must guard.
        var outcome = await _bus.InvokeAsync<Result>(
            new CancelMyServiceCommand(id, command.Type, command.Reason), ct);
        return HandleResultOk(outcome);
    }

    /// <summary>
    /// Sets up a pending service with hosting details (domain, username, password) and triggers
    /// provisioning.
    /// </summary>
    /// <param name="id">Client service primary key; wins over any id in the body.</param>
    /// <param name="command">The hosting details to set up.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 on successful setup and provisioning; 404 with <c>SERVICE_NOT_FOUND</c> when the
    /// service is not the caller's; 400 with <c>SERVICE_NOT_PENDING</c>; 503 with
    /// <c>NO_ELIGIBLE_SERVER</c>; 502 with <c>PROVISIONING_UPSTREAM_FAILED</c>; 401;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>PlaceNewOrders</c>.
    /// </returns>
    [HttpPost("{id:int}/setup")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.PlaceNewOrders)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetupAsync(int id, [FromBody] SetupMyServiceCommand command, CancellationToken ct)
    {
        // Reconstructed explicitly, not overlaid with `with` -- see the comment on CancelAsync.
        var outcome = await _bus.InvokeAsync<Result>(
            new SetupMyServiceCommand(id, command.Domain, command.Username, command.Password), ct);
        return HandleResultOk(outcome);
    }

    /// <summary>Returns the invoices charged to a service owned by the authenticated client.</summary>
    /// <remarks>
    /// The only action on this controller whose use case scopes itself to the caller. The reply
    /// carries the count of the caller's unattributable invoices alongside the list, because an
    /// empty list here means "nothing is recorded against this service", which is a weaker claim
    /// than "nothing was charged" — see <see cref="ServiceInvoicesDto"/>.
    /// </remarks>
    /// <param name="id">Client service primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the service's invoices; 401; 404 with <c>SERVICE_NOT_FOUND</c> when the
    /// service is not the caller's;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>ViewProductsServices</c> or
    /// <c>ViewPayInvoices</c>.
    /// </returns>
    [HttpGet("{id:int}/invoices")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.ViewProductsServices | ClientPermission.ViewPayInvoices)]
    [ProducesResponseType(typeof(ServiceInvoicesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoicesAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<ServiceInvoicesDto>>(new GetMyServiceInvoicesQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns the cancellation status for a client service.</summary>
    /// <param name="id">Client service primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the cancellation status; 401; 404 with <c>SERVICE_NOT_FOUND</c> when the
    /// service is not the caller's;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>ViewProductsServices</c>.
    /// </returns>
    [HttpGet("{id:int}/cancellation-status")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.ViewProductsServices)]
    [ProducesResponseType(typeof(CancellationStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCancellationStatusAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<CancellationStatusDto>>(
            new GetMyServiceCancellationStatusQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Changes the hosting account password for a client service.</summary>
    /// <param name="id">Client service primary key; wins over any id in the body.</param>
    /// <param name="command">The new password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 on success; 404 with <c>SERVICE_NOT_FOUND</c> when the service is not the caller's; 401;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>ViewModifyPasswords</c>.
    /// </returns>
    [HttpPost("{id:int}/change-password")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.ViewModifyPasswords)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePasswordAsync(
        int id, [FromBody] ChangeMyServicePasswordCommand command, CancellationToken ct)
    {
        // Reconstructed explicitly, not overlaid with `with` -- see the comment on CancelAsync.
        var outcome = await _bus.InvokeAsync<Result>(new ChangeMyServicePasswordCommand(id, command.NewPassword), ct);
        return HandleResultOk(outcome);
    }
}
