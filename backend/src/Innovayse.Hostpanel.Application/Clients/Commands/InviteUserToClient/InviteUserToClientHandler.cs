using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Domain.Notifications;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Clients.Commands.InviteUserToClient;

/// <summary>
/// Validates the invitation preconditions, creates an <see cref="Invitation"/> entity, seeds the
/// email template on first use, and sends the invitation email.
/// </summary>
public sealed class InviteUserToClientHandler
{
    /// <summary>The email template slug used for user invitations.</summary>
    private const string TemplateSlug = "user-invite";

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Reads the owner and any existing account with the invited address.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Wolverine message bus, for sending the invitation email.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Where the client portal lives, for the link in the invitation mail.</summary>
    private readonly IOptions<ClientPortalOptions> _clientPortal;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="identity">Reads the owner and any existing account with the invited address.</param>
    /// <param name="bus">Wolverine message bus, for sending the invitation email.</param>
    /// <param name="clientPortal">Where the client portal lives, for the link in the invitation mail.</param>
    public InviteUserToClientHandler(
        IHostpanelDbContext context,
        IIdentityProvider identity,
        IMessageBus bus,
        IOptions<ClientPortalOptions> clientPortal)
    {
        _context = context;
        _identity = identity;
        _bus = bus;
        _clientPortal = clientPortal;
    }

    /// <summary>Invites a user to a client account by creating an invitation and sending an email.</summary>
    /// <param name="cmd">The invite command with client id, email, name and permissions.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>CLIENT_NOT_FOUND</c>, <c>ACCOUNT_OWNER_NOT_FOUND</c>,
    /// <c>CANNOT_INVITE_ACCOUNT_OWNER</c>, <c>USER_ALREADY_LINKED</c> or
    /// <c>INVITATION_ALREADY_PENDING</c>.
    /// </returns>
    public async Task<Result> HandleAsync(InviteUserToClientCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var client = await _context.Clients
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == cmd.ClientId, ct);
        if (client is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        // The message deliberately omits client.UserId. This runs on a client-portal endpoint too,
        // and that id is the *owner's* internal identity key -- something the person inviting never
        // supplied and has no use for.
        var owner = await _identity.FindBySubjectAsync(client.UserId, ct);
        if (owner is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.ACCOUNT_OWNER_NOT_FOUND);
        }

        if (string.Equals(owner.Email, cmd.Email, StringComparison.OrdinalIgnoreCase))
        {
            return Result.FailureCode(HostpanelErrorCodes.CANNOT_INVITE_ACCOUNT_OWNER);
        }

        // An exact lookup: inviting "sam@example.com" to a client that already has
        // "samuel@example.com" linked must not be refused for a name that only shares a prefix.
        var existing = await _identity.FindByEmailAsync(cmd.Email, ct);
        if (existing is not null && client.Users.Any(u => u.UserId == existing.Subject))
        {
            return Result.FailureCode(HostpanelErrorCodes.USER_ALREADY_LINKED);
        }

        var pending = await _context.Invitations.FirstOrDefaultAsync(
            i => i.Email == cmd.Email && i.ClientId == cmd.ClientId && i.AcceptedAt == null && i.ExpiresAt > DateTimeOffset.UtcNow,
            ct);
        if (pending is not null)
        {
            return Result.FailureCode(HostpanelErrorCodes.INVITATION_ALREADY_PENDING);
        }

        var invitation = Invitation.Create(cmd.ClientId, cmd.Email, cmd.FirstName, cmd.LastName, (ClientPermission)cmd.Permissions);
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync(ct);

        await SeedInviteTemplateAsync(ct);

        var clientBaseUrl = _clientPortal.Value.BaseUrl;
        var inviteLink = $"{clientBaseUrl}/client/accept-invite?token={Uri.EscapeDataString(invitation.Token)}";

        await _bus.InvokeAsync(new SendEmailCommand(
            cmd.Email,
            TemplateSlug,
            new
            {
                invite_link = inviteLink,
                client_name = $"{client.FirstName} {client.LastName}",
                first_name = cmd.FirstName
            }), ct);

        return Result.Success();
    }

    /// <summary>Seeds the user invitation email template if it does not yet exist in the database.</summary>
    /// <param name="ct">Cancellation token.</param>
    private async Task SeedInviteTemplateAsync(CancellationToken ct)
    {
        var existing = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Slug == TemplateSlug, ct);
        if (existing is not null)
        {
            return;
        }

        var body = """
            <!DOCTYPE html>
            <html>
            <head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1.0"></head>
            <body style="margin:0;padding:0;background-color:#0a0a0f;font-family:'Inter',system-ui,-apple-system,sans-serif;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#0a0a0f;padding:40px 20px;">
                <tr><td align="center">
                  <table role="presentation" width="480" cellpadding="0" cellspacing="0" style="max-width:480px;width:100%;">
                    <tr><td align="center" style="padding-bottom:32px;">
                      <table role="presentation" cellpadding="0" cellspacing="0"><tr>
                        <td style="background:linear-gradient(135deg,rgba(14,165,233,0.1),rgba(168,85,247,0.1));border:1px solid rgba(14,165,233,0.2);border-radius:10px;padding:8px 16px;">
                          <span style="font-size:16px;font-weight:700;background:linear-gradient(135deg,#0ea5e9,#a855f7);-webkit-background-clip:text;-webkit-text-fill-color:transparent;background-clip:text;">Innovayse</span>
                        </td>
                      </tr></table>
                    </td></tr>
                    <tr><td style="background-color:#1a1a1f;border:1px solid rgba(255,255,255,0.06);border-radius:16px;padding:40px 36px;">
                      <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                        <tr><td align="center" style="padding-bottom:24px;">
                          <h1 style="margin:0;font-size:22px;font-weight:700;color:#f0f0f5;">You've Been Invited!</h1>
                        </td></tr>
                        <tr><td align="center" style="padding-bottom:12px;">
                          <p style="margin:0;font-size:14px;color:#8a8a9a;line-height:1.6;">Hi {{ first_name }},</p>
                        </td></tr>
                        <tr><td align="center" style="padding-bottom:28px;">
                          <p style="margin:0;font-size:14px;color:#8a8a9a;line-height:1.6;">You've been invited to manage the account for <strong style="color:#f0f0f5;">{{ client_name }}</strong>.</p>
                          <p style="margin:8px 0 0;font-size:14px;color:#8a8a9a;line-height:1.6;">Click the button below to set your password and get started:</p>
                        </td></tr>
                        <tr><td align="center" style="padding-bottom:28px;">
                          <a href="{{ invite_link }}" style="display:inline-block;padding:14px 32px;background:linear-gradient(135deg,#0ea5e9,#0284c7);color:#ffffff;font-size:15px;font-weight:600;text-decoration:none;border-radius:10px;box-shadow:0 4px 20px rgba(14,165,233,0.25);">Set Your Password</a>
                        </td></tr>
                        <tr><td style="border-top:1px solid rgba(255,255,255,0.06);padding-top:20px;">
                          <p style="margin:0;font-size:12px;color:#5a5a6a;line-height:1.6;">This link expires in 7 days. If you didn't expect this invitation, you can safely ignore this email.</p>
                          <p style="margin:8px 0 0;font-size:12px;color:#0ea5e9;word-break:break-all;">{{ invite_link }}</p>
                        </td></tr>
                      </table>
                    </td></tr>
                    <tr><td align="center" style="padding-top:24px;">
                      <p style="margin:0;font-size:11px;color:#3a3a4a;">© Innovayse. All rights reserved.</p>
                    </td></tr>
                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;

        var template = EmailTemplate.Create(TemplateSlug, "You've been invited — Innovayse", body,
            "Sent when a user is invited to manage a client account.");
        _context.EmailTemplates.Add(template);
        await _context.SaveChangesAsync(ct);
    }
}
