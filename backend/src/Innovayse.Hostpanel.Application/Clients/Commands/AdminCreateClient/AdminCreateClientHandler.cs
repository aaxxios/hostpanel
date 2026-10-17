using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Clients.Commands.AdminCreateClient;

/// <summary>
/// Creates a new <see cref="Client"/> aggregate, optionally provisioning a new account or linking
/// an existing one.
/// </summary>
public sealed class AdminCreateClientHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Reads the account when linking to an existing one.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Creates the account when asked to make a new one.</summary>
    private readonly IUserProvisioning _provisioning;

    /// <summary>Grants the new owner the <see cref="Roles.Client"/> role.</summary>
    private readonly ISubjectRoleStore _roles;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="identity">Reads the account when linking to an existing one.</param>
    /// <param name="provisioning">Creates the account when asked to make a new one.</param>
    /// <param name="roles">Grants the new owner the <see cref="Roles.Client"/> role.</param>
    public AdminCreateClientHandler(
        IHostpanelDbContext context,
        IIdentityProvider identity,
        IUserProvisioning provisioning,
        ISubjectRoleStore roles)
    {
        _context = context;
        _identity = identity;
        _provisioning = provisioning;
        _roles = roles;
    }

    /// <summary>
    /// Processes the admin client creation command. Creates or resolves the Identity user, builds
    /// the <see cref="Client"/> aggregate with all profile, address, notification and billing
    /// settings, then persists.
    /// </summary>
    /// <param name="cmd">The admin create client command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The new client's id, or <c>IDENTITY_USER_NOT_FOUND</c> when the existing user id names
    /// nobody, <c>EXISTING_USER_ALREADY_HAS_CLIENT</c> when that user already owns a client,
    /// <c>CURRENCY_NOT_FOUND</c> when the requested currency is not configured and enabled here, or
    /// <c>USER_PROVISIONING_NOT_ALLOWED_CREATE_ACCOUNT</c> when asked to create a new account where
    /// an SSO owns the accounts.
    /// </returns>
    public async Task<Result<int>> HandleAsync(AdminCreateClientCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        string userId;
        string email;

        if (cmd.CreateNewUser)
        {
            // The name goes in at creation rather than in a second write afterwards, so an admin
            // who left the language field alone still gets a client whose name is on the account.
            var created = await _provisioning.CreateAsync(cmd.Email!, cmd.Password!, cmd.FirstName, cmd.LastName, ct);
            if (created.IsFailure)
            {
                return Result<int>.FailureCode(created.ErrorCode!);
            }

            userId = created.Value!;
            await _roles.AddAsync(userId, Roles.Client, ct);

            if (cmd.Language is not null)
            {
                await _provisioning.UpdateProfileAsync(userId, cmd.FirstName, cmd.LastName, cmd.Language, ct);
            }

            email = cmd.Email!;
        }
        else
        {
            var existingUser = await _identity.FindBySubjectAsync(cmd.ExistingUserId!, ct);
            if (existingUser is null)
            {
                return Result<int>.FailureCode(HostpanelErrorCodes.IDENTITY_USER_NOT_FOUND);
            }

            var alreadyHasClient = await _context.Clients
                .AsNoTracking()
                .AnyAsync(c => c.UserId == cmd.ExistingUserId, ct);
            if (alreadyHasClient)
            {
                return Result<int>.FailureCode(HostpanelErrorCodes.EXISTING_USER_ALREADY_HAS_CLIENT);
            }

            userId = existingUser.Subject;
            email = cmd.Email ?? existingUser.Email;
        }

        var status = ParseStatus(cmd.Status);

        var client = Client.Create(userId, cmd.FirstName, cmd.LastName, email, cmd.CompanyName);

        client.Update(cmd.FirstName, cmd.LastName, cmd.CompanyName, cmd.Phone);
        client.UpdateAddress(cmd.Street, cmd.Address2, cmd.City, cmd.State, cmd.PostCode, cmd.Country);
        client.UpdatePreferences(cmd.PaymentMethod, cmd.BillingContact, cmd.AdminNotes);

        // A currency is chosen once. Left blank, the client has none recorded and is billed in the
        // base by the resolver; given, it must be one the panel actually offers, because this row
        // is what every later invoice is raised in.
        if (cmd.Currency is not null)
        {
            var code = cmd.Currency.ToUpperInvariant();
            var offered = await _context.Currencies.AsNoTracking()
                .AnyAsync(c => c.Code == code && c.IsEnabled, ct);
            if (!offered)
            {
                return Result<int>.FailureCode(HostpanelErrorCodes.CURRENCY_NOT_FOUND);
            }

            client.SetCurrency(code);
        }

        client.UpdateNotifications(
            cmd.NotifyGeneral, cmd.NotifyInvoice, cmd.NotifySupport,
            cmd.NotifyProduct, cmd.NotifyDomain, cmd.NotifyAffiliate);

        client.UpdateSettings(
            cmd.LateFees, cmd.OverdueNotices, cmd.TaxExempt, cmd.SeparateInvoices,
            cmd.DisableCcProcessing, cmd.MarketingOptIn, cmd.StatusUpdate, cmd.AllowSso);

        // Applies a non-Active status if specified (Client.Create defaults to Active).
        ApplyStatus(client, status);

        _context.Clients.Add(client);
        await _context.SaveChangesAsync(ct);

        return Result<int>.Success(client.Id);
    }

    /// <summary>Parses a status string to <see cref="ClientStatus"/>, defaulting to <see cref="ClientStatus.Active"/>.</summary>
    /// <param name="status">The status string to parse, or <see langword="null"/>.</param>
    /// <returns>The parsed <see cref="ClientStatus"/> value.</returns>
    private static ClientStatus ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return ClientStatus.Active;
        }

        return Enum.TryParse<ClientStatus>(status, ignoreCase: true, out var parsed) ? parsed : ClientStatus.Active;
    }

    /// <summary>
    /// Applies the desired status to the client aggregate using domain methods. Skips if the
    /// status is already <see cref="ClientStatus.Active"/> (the default).
    /// </summary>
    /// <param name="client">The client aggregate to update.</param>
    /// <param name="status">The desired status.</param>
    private static void ApplyStatus(Client client, ClientStatus status)
    {
        switch (status)
        {
            case ClientStatus.Active:
                break;
            case ClientStatus.Inactive:
                // No domain method for Inactive -- the domain only supports Active, Suspended and
                // Closed transitions, so an Inactive request leaves the client Active.
                break;
            case ClientStatus.Suspended:
                client.Suspend();
                break;
            case ClientStatus.Closed:
                client.Suspend();
                client.Close();
                break;
        }
    }
}
