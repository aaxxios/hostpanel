using Innovayse.Hostpanel.Domain.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;
/// <summary>EF Core configuration for the <see cref="Nameserver"/> entity.</summary>
public sealed class NameserverConfiguration : IEntityTypeConfiguration<Nameserver>
{
    /// <summary>Configures the <c>Nameservers</c> table mapping.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Nameserver> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DomainId).IsRequired();
        builder.Property(x => x.Host).IsRequired().HasMaxLength(255);
    }
}
