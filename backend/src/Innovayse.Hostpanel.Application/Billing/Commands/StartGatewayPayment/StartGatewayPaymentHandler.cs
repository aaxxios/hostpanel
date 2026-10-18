using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Constants;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.SDK.Plugins;
using Innovayse.SDK.Plugins.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace Innovayse.Hostpanel.Application.Billing.Commands.StartGatewayPayment;

/// <summary>
/// Handles <see cref="StartGatewayPaymentCommand"/>: resolves the payment plugin,
/// registers the payment (fresh per-attempt order number — the gateway rejects
/// reused ones), stores the session on the invoice, and returns the redirect URL.
/// </summary>
public sealed class StartGatewayPaymentHandler
{
    /// <summary>
    /// How long a started gateway session is considered "live" (matches the hosted gateway's own
    /// hosted-page session lifetime). A second <c>start</c> for the same invoice inside this
    /// window is refused unless the existing session is already known to be Declined.
    /// </summary>
    private const int SessionWindowMinutes = 20;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the configured payment plugin for a module id.</summary>
    private readonly IPaymentPluginResolver _pluginResolver;

    /// <summary>The origins a payer may be handed back to, used to validate <c>ReturnUrl</c>.</summary>
    private readonly IOptions<GatewayReturnUrlOptions> _returnUrlOptions;

    /// <summary>
    /// Structured logger, used to record gateway status-probe failures during the live-session
    /// check.
    /// </summary>
    private readonly ILogger<StartGatewayPaymentHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="pluginResolver">Resolves the configured payment plugin for a module id.</param>
    /// <param name="returnUrlOptions">The origins a payer may be handed back to.</param>
    /// <param name="logger">Structured logger.</param>
    public StartGatewayPaymentHandler(
        IHostpanelDbContext context,
        IPaymentPluginResolver pluginResolver,
        IOptions<GatewayReturnUrlOptions> returnUrlOptions,
        ILogger<StartGatewayPaymentHandler> logger)
    {
        _context = context;
        _pluginResolver = pluginResolver;
        _returnUrlOptions = returnUrlOptions;
        _logger = logger;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The start command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The gateway redirect URL for the payer's browser, or one of: <c>INVOICE_NOT_FOUND</c>,
    /// <c>INVOICE_NOT_PAYABLE</c>, <c>RETURN_URL_NOT_ALLOWED</c>,
    /// <c>PAYMENT_PLUGIN_NOT_CONFIGURED</c>, <c>GATEWAY_SESSION_ALREADY_STARTED</c>,
    /// <c>CURRENCY_NOT_FOUND</c> or <c>GATEWAY_CURRENCY_MISMATCH</c>.
    /// </returns>
    public async Task<Result<string>> HandleAsync(StartGatewayPaymentCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId, ct);
        if (invoice is null)
        {
            return Result<string>.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        // Check payability before touching the bank at all — starting a session for a
        // Paid/Cancelled/Refunded invoice would register a live payable order at the gateway
        // for nothing, since SetGatewaySession() would reject it afterwards anyway.
        if (invoice.Status is not (InvoiceStatus.Unpaid or InvoiceStatus.Overdue))
        {
            return Result<string>.FailureCode(HostpanelErrorCodes.INVOICE_NOT_PAYABLE);
        }

        if (!IsReturnUrlAllowed(cmd.ReturnUrl))
        {
            return Result<string>.FailureCode(HostpanelErrorCodes.RETURN_URL_NOT_ALLOWED);
        }

        var plugin = await _pluginResolver.ResolveAsync(cmd.Module, ct);
        if (plugin is null)
        {
            return Result<string>.FailureCode(HostpanelErrorCodes.PAYMENT_PLUGIN_NOT_CONFIGURED);
        }

        if (await HasLiveSessionAsync(invoice, ct))
        {
            return Result<string>.FailureCode(HostpanelErrorCodes.GATEWAY_SESSION_ALREADY_STARTED);
        }

        var invoiceCurrency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == invoice.Currency, ct);
        if (invoiceCurrency is null)
        {
            return Result<string>.FailureCode(HostpanelErrorCodes.CURRENCY_NOT_FOUND);
        }

        if (!plugin.AcceptsCurrency(invoiceCurrency.Numeric))
        {
            return Result<string>.FailureCode(HostpanelErrorCodes.GATEWAY_CURRENCY_MISMATCH);
        }

        // Unique per attempt (gateway errorCode 1 on reuse); ≤ 25 chars fits int ids + unix seconds.
        var orderNumber = $"INV{invoice.Id}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        // The Application layer owns the major→minor conversion (ISO 4217 exponent-2).
        var amountMinor = CurrencyCodes.ToMinorUnits(invoice.Total);

        var session = await plugin.CreatePaymentAsync(
            new PaymentRequest(orderNumber, amountMinor, cmd.ReturnUrl, $"Invoice #{invoice.Id}", null),
            ct);

        invoice.SetGatewaySession(cmd.Module, session.GatewayOrderId);
        await _context.SaveChangesAsync(ct);

        return Result.Success(session.RedirectUrl);
    }

    /// <summary>
    /// Whether the invoice's existing session is still inside the gateway's live window and is
    /// not already known to be Declined. When the gateway's status API cannot be reached at all
    /// (e.g. a bank outage), the session's true state is unknowable — this is treated the same
    /// as "still live" rather than silently allowing a second session that could orphan a real
    /// payment at the gateway.
    /// </summary>
    /// <param name="invoice">The invoice being paid.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> when a new session must be refused.</returns>
    private async Task<bool> HasLiveSessionAsync(Invoice invoice, CancellationToken ct)
    {
        if (invoice.GatewayStartedAt is not { } startedAt)
        {
            return false;
        }

        var age = DateTimeOffset.UtcNow - startedAt;
        if (age >= TimeSpan.FromMinutes(SessionWindowMinutes))
        {
            return false;
        }

        var isDeclined = false;
        if (invoice.GatewayModule is not null && invoice.GatewayOrderId is not null)
        {
            var previousPlugin = await _pluginResolver.ResolveAsync(invoice.GatewayModule, ct);
            if (previousPlugin is not null)
            {
                try
                {
                    var status = await previousPlugin.GetStatusAsync(invoice.GatewayOrderId, ct);
                    isDeclined = status.State == GatewayPaymentState.Declined;
                }
                catch (OperationCanceledException)
                {
                    // The caller cancelled the request itself — not a gateway failure. Let it
                    // propagate rather than reinterpreting it as "session may still be live".
                    throw;
                }
                catch (Exception ex)
                {
                    // Cannot determine the previous session's state — refuse below rather than
                    // risk a second live session at the gateway. An operator needs to see why
                    // payers are being told to wait, so this is logged rather than swallowed.
                    _logger.LogWarning(
                        ex,
                        "Could not determine gateway status for invoice {InvoiceId} session {GatewayOrderId}; " +
                        "treating the existing session as still live.",
                        invoice.Id, invoice.GatewayOrderId);
                }
            }
        }

        return !isDeclined;
    }

    /// <summary>
    /// Whether <paramref name="returnUrl"/>'s origin is one of the panel's configured CORS
    /// origins, so a payer can never be handed off to an attacker-controlled domain.
    /// </summary>
    /// <param name="returnUrl">The absolute URL the payer will be redirected back to.</param>
    /// <returns><see langword="true"/> when the URL is absolute and its origin is allowed.</returns>
    private bool IsReturnUrlAllowed(string returnUrl)
    {
        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var origin = $"{uri.Scheme}://{uri.Authority}";
        return _returnUrlOptions.Value.AllowedOrigins.Any(o =>
            string.Equals(o.TrimEnd('/'), origin, StringComparison.OrdinalIgnoreCase));
    }
}
