using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Rendering;
using Innovayse.Hostpanel.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Notifications.Seeding;

/// <summary>
/// Lazily seeds the shared "user-email-verification" email template on first use. Used by the
/// registration flow and by the resend use case through <c>SendVerificationEmailHandler</c>, so
/// both send an identical email instead of drifting apart.
/// <para>
/// The layout follows the brand through the <c>brand.*</c> and <c>site.*</c> objects
/// <see cref="TemplateRenderer"/> adds to every render, exactly like the password-reset template.
/// The row is seeded once and never overwritten; an install that already has it keeps its stored
/// HTML until the operator re-saves the template from the admin panel.
/// </para>
/// </summary>
public static class EmailVerificationTemplateSeeder
{
    /// <summary>The template slug the verification mail is sent under.</summary>
    public const string Slug = "user-email-verification";

    /// <summary>
    /// Ensures the email verification template exists, creating it with the default
    /// design if this is the first verification ever requested.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="uow">Unit of work for persisting the template if newly created.</param>
    /// <param name="ct">Cancellation token.</param>
    public static async Task EnsureSeededAsync(
        IHostpanelDbContext context,
        IUnitOfWork uow,
        CancellationToken ct)
    {
        // Was IEmailTemplateRepository.FindBySlugAsync -- read-only, so no tracking is needed.
        var existing = await context.EmailTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == Slug, ct);
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
                      {% if brand.logo_url != "" %}
                      <img src="{{ brand.logo_url }}" alt="{{ site.name }}" height="40" style="display:block;height:40px;width:auto;max-width:200px;border:0;" />
                      {% else %}
                      <table role="presentation" cellpadding="0" cellspacing="0"><tr>
                        <td style="border:1px solid {{ brand.primary }};border-radius:10px;padding:8px 16px;">
                          <span style="font-size:16px;font-weight:700;color:{{ brand.primary }};">{{ site.name }}</span>
                        </td>
                      </tr></table>
                      {% endif %}
                    </td></tr>
                    <tr><td style="background-color:#1a1a1f;border:1px solid rgba(255,255,255,0.06);border-radius:16px;padding:40px 36px;">
                      <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                        <tr><td align="center" style="padding-bottom:24px;">
                          <h1 style="margin:0;font-size:22px;font-weight:700;color:#f0f0f5;">Verify Your Email</h1>
                        </td></tr>
                        <tr><td align="center" style="padding-bottom:28px;">
                          <p style="margin:0;font-size:14px;color:#8a8a9a;line-height:1.6;">Click the button below to confirm your email address and finish setting up your account.</p>
                        </td></tr>
                        <tr><td align="center" style="padding-bottom:28px;">
                          <a href="{{ verification_link }}" style="display:inline-block;padding:14px 32px;background:linear-gradient(135deg,{{ brand.primary }},{{ brand.primary_dark }});color:{{ brand.on_primary }};font-size:15px;font-weight:600;text-decoration:none;border-radius:10px;box-shadow:0 4px 20px rgba(14,165,233,0.25);">Verify Email</a>
                        </td></tr>
                        <tr><td style="border-top:1px solid rgba(255,255,255,0.06);padding-top:20px;">
                          <p style="margin:0;font-size:12px;color:#5a5a6a;line-height:1.6;">If you didn't create this account, you can safely ignore this email.</p>
                          <p style="margin:8px 0 0;font-size:12px;color:{{ brand.primary }};word-break:break-all;">{{ verification_link }}</p>
                        </td></tr>
                      </table>
                    </td></tr>
                    <tr><td align="center" style="padding-top:24px;">
                      <p style="margin:0;font-size:11px;color:#3a3a4a;">© {{ site.name }}. All rights reserved.</p>
                    </td></tr>
                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;

        var template = EmailTemplate.Create(Slug, "Verify your email — {{ site.name }}", body,
            "Sent after a local registration, and when a user asks for the verification link again.");
        context.EmailTemplates.Add(template);
        await uow.SaveChangesAsync(ct);
    }
}
