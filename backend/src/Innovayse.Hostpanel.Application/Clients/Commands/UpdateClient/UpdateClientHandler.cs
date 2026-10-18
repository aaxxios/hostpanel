using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Clients.Commands.UpdateClient;

/// <summary>
/// Loads the client aggregate, applies profile, address, preference, notification, settings and
/// status changes, then saves. Optionally updates the account's email and language.
/// </summary>
public sealed class UpdateClientHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    /// Writes the account-side fields -- the sign-in address and the chosen UI language -- where
    /// this deployment owns the account.
    /// </summary>
    private readonly IUserProvisioning _provisioning;

    /// <summary>
    /// Reads the address the account currently signs in with, so a save that did not touch it can
    /// be told apart from one that did.
    /// </summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Refuses credential changes on the published demo accounts.</summary>
    private readonly IDemoAccountGuard _demoAccounts;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="provisioning">Writes the account-side fields where this deployment owns the account.</param>
    /// <param name="identity">Reads the address the account currently signs in with.</param>
    /// <param name="demoAccounts">Refuses credential changes on the published demo accounts.</param>
    public UpdateClientHandler(
        IHostpanelDbContext context, IUserProvisioning provisioning, IIdentityProvider identity, IDemoAccountGuard demoAccounts)
    {
        _demoAccounts = demoAccounts;
        _context = context;
        _provisioning = provisioning;
        _identity = identity;
    }

    /// <summary>
    /// Updates the client's profile, billing address, preferences, notifications, settings and status.
    /// </summary>
    /// <param name="cmd">The update command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>CLIENT_NOT_FOUND</c> when the id names no client,
    /// <c>CLIENT_CURRENCY_ALREADY_INVOICED</c> when the currency is changed on a client who has
    /// already been invoiced, <c>CURRENCY_NOT_FOUND</c> when the requested currency is not one
    /// this panel offers, <c>CLIENT_STATUS_INVALID</c> when the status string names no member of
    /// <see cref="ClientStatus"/>, or <c>USER_PROVISIONING_NOT_ALLOWED_CHANGE_EMAIL</c> when the
    /// account's sign-in address is changed where an SSO owns the accounts, or
    /// <c>DEMO_ACTION_DISABLED</c> when the address of a published demo account is changed on a demo deployment.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateClientCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var client = await _context.Clients
            .IncludeChildren()
            .FirstOrDefaultAsync(c => c.Id == cmd.ClientId, ct);
        if (client is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        client.Update(cmd.FirstName, cmd.LastName, cmd.CompanyName, cmd.Phone);
        client.UpdateAddress(cmd.Street, cmd.Address2, cmd.City, cmd.State, cmd.PostCode, cmd.Country);
        client.UpdatePreferences(cmd.PaymentMethod, cmd.BillingContact, cmd.AdminNotes);

        var currencyOutcome = await ApplyCurrencyAsync(client, cmd.Currency, ct);
        if (currencyOutcome.IsFailure)
        {
            return currencyOutcome;
        }

        client.UpdateNotifications(cmd.NotifyGeneral, cmd.NotifyInvoice, cmd.NotifySupport, cmd.NotifyProduct, cmd.NotifyDomain, cmd.NotifyAffiliate);
        client.UpdateSettings(cmd.LateFees, cmd.OverdueNotices, cmd.TaxExempt, cmd.SeparateInvoices, cmd.DisableCcProcessing, cmd.MarketingOptIn, cmd.StatusUpdate, cmd.AllowSso);

        if (cmd.Status is not null)
        {
            // The validator already refuses an unparsable status string, so this defends a direct
            // caller rather than one that reached here through the API.
            if (!Enum.TryParse<ClientStatus>(cmd.Status, ignoreCase: true, out var newStatus))
            {
                return Result.FailureCode(HostpanelErrorCodes.CLIENT_STATUS_INVALID);
            }

            if (newStatus != client.Status)
            {
                switch (newStatus)
                {
                    case ClientStatus.Active:
                        client.Activate();
                        break;
                    case ClientStatus.Suspended:
                        client.Suspend();
                        break;
                    case ClientStatus.Closed:
                        client.Close();
                        break;
                    case ClientStatus.Inactive:
                        // Inactive is set by Suspend or the initial state -- no dedicated domain method.
                        break;
                }
            }
        }

        // Read once and shared by both account-side writes below: the client record holds neither
        // the sign-in address nor the language, so "has this actually changed?" can only be
        // answered against whoever owns the person.
        var account = string.IsNullOrEmpty(client.UserId)
            ? null
            : await _identity.FindBySubjectAsync(client.UserId, ct);

        // The account's sign-in address, if this deployment is the one that owns it. Only the
        // address: the names belong to the client record, updated above. Asked as "has the
        // address changed?", never as "was an address sent?" -- the account form posts it on
        // every save because it is a populated field on the form.
        if (!string.IsNullOrWhiteSpace(cmd.Email) && !string.IsNullOrEmpty(client.UserId))
        {
            if (account is null || !string.Equals(account.Email, cmd.Email, StringComparison.OrdinalIgnoreCase))
            {
                // The one rule that keeps the published demo accounts signable: their email is frozen.
                var allowed = await _demoAccounts.EnsureCredentialsUnchangedAsync(client.UserId, ct);
                if (allowed.IsFailure)
                {
                    return allowed;
                }

                var emailChanged = await _provisioning.ChangeEmailAsync(client.UserId, cmd.Email, ct);
                if (emailChanged.IsFailure)
                {
                    return emailChanged;
                }
            }
        }

        // The account holder's chosen UI language, written through the same provisioner as the
        // address and only where this deployment owns the person. Null means "leave it alone" and
        // blank means "no preference", stored as null.
        if (cmd.Language is not null && account is not null)
        {
            var language = string.IsNullOrWhiteSpace(cmd.Language) ? null : cmd.Language;

            if (!string.Equals(account.Language, language, StringComparison.OrdinalIgnoreCase))
            {
                // Where an SSO owns the person it owns their language too. A refusal here is
                // ignored rather than propagated: a UI language is a preference the form posts
                // alongside everything else, and failing the whole save over it would refuse a
                // customer editing a field they never touched.
                await _provisioning.UpdateProfileAsync(client.UserId, account.FirstName, account.LastName, language, ct);
            }
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    /// <summary>Moves the client to the requested currency, when that is still allowed.</summary>
    /// <remarks>
    /// Asked as "has the currency changed?", never as "was a currency sent?": the account form
    /// posts the field on every save, and a client who has been invoiced must still be able to
    /// save their phone number. Changing the code would re-label every existing invoice without
    /// converting an amount -- $100 would read as ֏100 -- so once billed, the currency is fixed.
    /// </remarks>
    /// <param name="client">The client under edit.</param>
    /// <param name="requested">The currency the form posted, or <see langword="null"/> to leave it alone.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>CLIENT_CURRENCY_ALREADY_INVOICED</c> when the client has already been
    /// invoiced, or <c>CURRENCY_NOT_FOUND</c> when the currency is not configured and enabled.
    /// </returns>
    private async Task<Result> ApplyCurrencyAsync(Client client, string? requested, CancellationToken ct)
    {
        if (requested is null || string.Equals(requested, client.Currency, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Success();
        }

        var alreadyInvoiced = await _context.Invoices.AsNoTracking().AnyAsync(i => i.ClientId == client.Id, ct);
        if (alreadyInvoiced)
        {
            return Result.FailureCode(HostpanelErrorCodes.CLIENT_CURRENCY_ALREADY_INVOICED);
        }

        var code = requested.ToUpperInvariant();
        var offered = await _context.Currencies.AsNoTracking().AnyAsync(c => c.Code == code && c.IsEnabled, ct);
        if (!offered)
        {
            return Result.FailureCode(HostpanelErrorCodes.CURRENCY_NOT_FOUND);
        }

        client.SetCurrency(code);
        return Result.Success();
    }
}
