using Innovayse.Hostpanel.Domain.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="DnsRecord"/> entity.</summary>
public sealed class DnsRecordConfiguration : IEntityTypeConfiguration<DnsRecord>
{
    /// <summary>Configures the <c>DnsRecords</c> table mapping.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<DnsRecord> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DomainId).IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(x => x.Host).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Value).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Ttl).IsRequired();
        builder.Property(x => x.Priority);
    }
}
