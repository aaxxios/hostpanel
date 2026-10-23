using Innovayse.Hostpanel.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;
/// <summary>EF Core configuration for the <see cref="EmailLog"/> entity.</summary>
public sealed class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    /// <summary>Configures the <c>EmailLogs</c> table mapping.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.To).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Body).IsRequired();
        builder.Property(x => x.SentAt).IsRequired();
        builder.Property(x => x.Success).IsRequired();
        builder.Property(x => x.Error).HasMaxLength(1000);
    }
}
