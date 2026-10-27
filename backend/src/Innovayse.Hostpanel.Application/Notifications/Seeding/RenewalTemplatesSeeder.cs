using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Notifications.Seeding;

/// <summary>
/// Seeds the two templates the billing engine sends, the first time each is needed (the pattern of
/// <see cref="EmailVerificationTemplateSeeder"/>): a migration-seeded row would freeze the copy, while
/// a runtime seed lets an operator edit it and never overwrites the edit.
/// </summary>
public static class RenewalTemplatesSeeder
{
    /// <summary>Slug of the client renewal reminder.</summary>
    public const string ReminderSlug = "invoice-renewal-reminder";

    /// <summary>Slug of the staff alert about a domain renewal that needs a person.</summary>
    public const string DomainFailedAdminSlug = "domain-renewal-failed-admin";

    /// <summary>Adds whichever of the two templates is missing.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="uow">Unit of work.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    public static async Task EnsureSeededAsync(IHostpanelDbContext context, IUnitOfWork uow, CancellationToken ct)
    {
        var existing = await context.EmailTemplates.AsNoTracking()
            .Where(t => t.Slug == ReminderSlug || t.Slug == DomainFailedAdminSlug)
            .Select(t => t.Slug)
            .ToListAsync(ct);

        if (!existing.Contains(ReminderSlug))
        {
            context.EmailTemplates.Add(EmailTemplate.Create(
                ReminderSlug,
                "Invoice #{{ invoice.id }} is due on {{ invoice.due_date }} — {{ site.name }}",
                Shell("Your renewal is coming up",
                    "Invoice #{{ invoice.id }} for {{ invoice.total }} {{ invoice.currency }} is due on {{ invoice.due_date }}. Pay it to keep your services running without interruption.",
                    "{{ invoice.url }}", "View invoice"),
                "Sent before a renewal invoice is due, on each day listed in billing.renewal.reminder_days."));
        }

        if (!existing.Contains(DomainFailedAdminSlug))
        {
            context.EmailTemplates.Add(EmailTemplate.Create(
                DomainFailedAdminSlug,
                "Domain renewal needs attention: {{ domain.name }}",
                Shell("Domain renewal needs attention",
                    // invoice.id is 0 when no invoice was raised (an unpriceable domain), so it is shown only when real.
                    "{{ domain.name }} (client #{{ client.id }}{% if invoice.id != 0 %}, invoice #{{ invoice.id }}{% endif %}): {{ reason }}",
                    null, null),
                "Sent to staff when a paid or due domain renewal could not be completed automatically."));
        }

        if (existing.Count < 2)
        {
            await uow.SaveChangesAsync(ct);
        }
    }

    /// <summary>The shared branded HTML shell (copied from the verification template).</summary>
    /// <param name="heading">Card heading.</param>
    /// <param name="paragraph">Card text (Liquid allowed).</param>
    /// <param name="buttonUrl">Button link, or null for none.</param>
    /// <param name="buttonText">Button text, or null for none.</param>
    /// <returns>The HTML body.</returns>
    private static string Shell(string heading, string paragraph, string? buttonUrl, string? buttonText)
    {
        // Three $: Liquid's own {{ }} must stay literal, so only {{{ }}} holes interpolate.
        var button = buttonUrl is null || buttonText is null
            ? string.Empty
            : $$$"""
                        <tr><td align="center" style="padding-bottom:28px;">
                          <a href="{{{buttonUrl}}}" style="display:inline-block;padding:14px 32px;background:linear-gradient(135deg,{{ brand.primary }},{{ brand.primary_dark }});color:{{ brand.on_primary }};font-size:15px;font-weight:600;text-decoration:none;border-radius:10px;box-shadow:0 4px 20px rgba(14,165,233,0.25);">{{{buttonText}}}</a>
                        </td></tr>

            """;

        return $$$"""
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
                          <h1 style="margin:0;font-size:22px;font-weight:700;color:#f0f0f5;">{{{heading}}}</h1>
                        </td></tr>
                        <tr><td align="center" style="padding-bottom:28px;">
                          <p style="margin:0;font-size:14px;color:#8a8a9a;line-height:1.6;">{{{paragraph}}}</p>
                        </td></tr>
            {{{button}}}          </table>
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
    }
}
