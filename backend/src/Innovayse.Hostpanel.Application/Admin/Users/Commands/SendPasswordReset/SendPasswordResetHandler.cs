using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Application.Notifications.Seeding;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.Options;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Admin.Users.Commands.SendPasswordReset;

/// <summary>
/// Handles <see cref="SendPasswordResetCommand"/>: issues a reset token, seeds the shared
/// email template on first use, builds the reset link, and dispatches the mail.
/// </summary>
public sealed class SendPasswordResetHandler
{
    /// <summary>Reads the person, for the address the mail goes to.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Issues the reset token, where this deployment owns the accounts.</summary>
    private readonly IUserProvisioning _provisioning;

    /// <summary>The module's data context, for seeding the reset template.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work for persisting the template if newly created.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Wolverine message bus, for sending the mail.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Where the client portal lives, for the link in the reset mail.</summary>
    private readonly IOptions<ClientPortalOptions> _clientPortal;

    /// <summary>Initializes the handler.</summary>
    /// <param name="identity">Reads the person, for the address the mail goes to.</param>
    /// <param name="provisioning">Issues the reset token, where this deployment owns the accounts.</param>
    /// <param name="context">The module's data context, for seeding the reset template.</param>
    /// <param name="uow">Unit of work for persisting the template if newly created.</param>
    /// <param name="bus">Wolverine message bus, for sending the mail.</param>
    /// <param name="clientPortal">Where the client portal lives, for the link in the reset mail.</param>
    public SendPasswordResetHandler(
        IIdentityProvider identity,
        IUserProvisioning provisioning,
        IHostpanelDbContext context,
        IUnitOfWork uow,
        IMessageBus bus,
        IOptions<ClientPortalOptions> clientPortal)
    {
        _identity = identity;
        _provisioning = provisioning;
        _context = context;
        _uow = uow;
        _bus = bus;
        _clientPortal = clientPortal;
    }

    /// <summary>Issues the token and sends the reset mail.</summary>
    /// <param name="cmd">The subject to send the reset link to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>IDENTITY_USER_NOT_FOUND</c> when the subject names no identity account, or
    /// <c>USER_PROVISIONING_NOT_ALLOWED_ISSUE_PASSWORD_RESET</c> where an SSO owns the accounts.
    /// </returns>
    public async Task<Result> HandleAsync(SendPasswordResetCommand cmd, CancellationToken ct)
    {
        var account = await _identity.FindBySubjectAsync(cmd.Id, ct);
        if (account is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.IDENTITY_USER_NOT_FOUND);
        }

        // The token first: it is the half that refuses where an SSO owns the account, and
        // seeding a template and sending mail before finding that out would leave an
        // operator with a delivered reset link that resets nothing.
        var tokenOutcome = await _provisioning.IssuePasswordResetTokenAsync(cmd.Id, ct);
        if (tokenOutcome.IsFailure)
        {
            return Result.FailureCode(tokenOutcome.ErrorCode!);
        }

        if (tokenOutcome.Value is null)
        {
            // The identity provider and the local account store disagree, which only a race
            // between this lookup and a delete elsewhere in the same instant could cause.
            return Result.FailureCode(HostpanelErrorCodes.IDENTITY_USER_NOT_FOUND);
        }

        var token = tokenOutcome.Value;

        await PasswordResetTemplateSeeder.EnsureSeededAsync(_context, _uow, ct);

        var clientBaseUrl = _clientPortal.Value.BaseUrl;
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(account.Email);
        var resetLink = $"{clientBaseUrl}/client/reset-password?token={encodedToken}&email={encodedEmail}";

        await _bus.InvokeAsync(new SendEmailCommand(
            account.Email,
            PasswordResetTemplateSeeder.Slug,
            new { reset_link = resetLink }), ct);

        return Result.Success();
    }
}
