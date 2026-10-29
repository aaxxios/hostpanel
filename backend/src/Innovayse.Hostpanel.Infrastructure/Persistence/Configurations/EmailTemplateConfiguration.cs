using Innovayse.Hostpanel.Domain.Notifications;
using Innovayse.Hostpanel.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="EmailTemplate"/> aggregate.</summary>
public sealed class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    /// <summary>Configures the <c>EmailTemplates</c> table mapping.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug).HasMaxLength(100).IsRequired();

        // Rows written before templates had a language were all written in the default one, so the
        // column's default is what they take when it is added; new rows always set it explicitly.
        builder.Property(x => x.Language).HasMaxLength(10).IsRequired().HasDefaultValue(SupportedLanguages.Default);
        builder.Property(x => x.Subject).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Body).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.IsActive).IsRequired();

        // One version per language of each template: the send picks the recipient's language by
        // this pair and falls back to the default language's row.
        builder.HasIndex(x => new { x.Slug, x.Language }).IsUnique();

        builder.HasData(
            new
            {
                Id = 1,
                Slug = "welcome",
                Language = SupportedLanguages.Default,
                Subject = "Welcome to {{ site.name }}!",
                Body = "<h1>Welcome!</h1><p>Hello, your account has been created successfully.</p>",
                IsActive = true,
                Description = "Sent on client registration",
            },
            new
            {
                Id = 2,
                Slug = "invoice-created",
                Language = SupportedLanguages.Default,
                Subject = "Invoice #{{invoice.id}} Created",
                Body = "<p>Your invoice for {{invoice.total}} {{invoice.currency}} is ready.</p>",
                IsActive = true,
                Description = "Sent when invoice created",
            },
            new
            {
                Id = 3,
                Slug = "payment-received",
                Language = SupportedLanguages.Default,
                Subject = "Payment Received",
                Body = "<p>Thank you for your payment of {{invoice.total}}.</p>",
                IsActive = true,
                Description = "Sent on payment",
            },
            new
            {
                Id = 4,
                Slug = "service-provisioned",
                Language = SupportedLanguages.Default,
                Subject = "Your Service is Ready",
                Body = "<p>Your hosting service has been provisioned.</p>",
                IsActive = true,
                Description = "Sent when service provisioned",
            },
            new
            {
                Id = 5,
                Slug = "ticket-created",
                Language = SupportedLanguages.Default,
                Subject = "Support Ticket #{{ticket.id}} Created",
                Body = "<p>We received your support request.</p>",
                IsActive = true,
                Description = "Sent when ticket created",
            },
            new
            {
                Id = 6,
                Slug = "ticket-replied",
                Language = SupportedLanguages.Default,
                Subject = "New Reply on Ticket #{{ticket.id}}: {{ticket.subject}}",
                Body = "<p>There's a new reply on your support ticket.</p>",
                IsActive = true,
                Description = "Sent when a reply is added to a ticket",
            },
            new
            {
                Id = 7,
                Slug = "ticket-closed",
                Language = SupportedLanguages.Default,
                Subject = "Support Ticket #{{ticket.id}} Closed",
                Body = "<p>Your support ticket \"{{ticket.subject}}\" has been closed. We'd love your feedback.</p>",
                IsActive = true,
                Description = "Sent when a ticket is closed",
            });
    }
}
