using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;
/// <summary>EF Core table configuration for <see cref="Server"/>.</summary>
public sealed class ServerConfiguration : IEntityTypeConfiguration<Server>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Server> builder)
    {
        builder.ToTable("servers");
        builder.HasKey(x => x.Id);

        var converter = EncryptionServiceHolder.CreateConverter();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Hostname).IsRequired().HasMaxLength(500);
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.AssignedIpAddresses).HasMaxLength(2000);
        builder.Property(x => x.Module).IsRequired().HasConversion<string>().HasMaxLength(100);
        builder.Property(x => x.Username).IsRequired().HasMaxLength(200);

        // Widths are sized for the encrypted form and are the same whether or not a key is
        // configured, so the model (and therefore the migration snapshot) never depends on the
        // environment it is built in. The converter alone is what varies.
        var password = builder.Property(x => x.Password).HasMaxLength(2000);
        var apiToken = builder.Property(x => x.ApiToken).HasMaxLength(2000);
        var accessHash = builder.Property(x => x.AccessHash).HasMaxLength(4000);
        if (converter is not null)
        {
            password.HasConversion(converter);
            apiToken.HasConversion(converter);
            accessHash.HasConversion(converter);
        }

        builder.Property(x => x.UseSSL).IsRequired();
        builder.Property(x => x.MaxAccounts);
        builder.Property(x => x.IsDefault).IsRequired();
        builder.Property(x => x.IsDisabled).IsRequired();
        builder.Property(x => x.MonthlyCost).IsRequired().HasColumnType("numeric(18,4)");
        builder.Property(x => x.Datacenter).HasMaxLength(200);
        builder.Property(x => x.ServerStatusAddress).HasMaxLength(500);
        builder.Property(x => x.Ns1Hostname).HasMaxLength(200);
        builder.Property(x => x.Ns1Ip).HasMaxLength(45);
        builder.Property(x => x.Ns2Hostname).HasMaxLength(200);
        builder.Property(x => x.Ns2Ip).HasMaxLength(45);
        builder.Property(x => x.Ns3Hostname).HasMaxLength(200);
        builder.Property(x => x.Ns3Ip).HasMaxLength(45);
        builder.Property(x => x.Ns4Hostname).HasMaxLength(200);
        builder.Property(x => x.Ns4Ip).HasMaxLength(45);
        builder.Property(x => x.Ns5Hostname).HasMaxLength(200);
        builder.Property(x => x.Ns5Ip).HasMaxLength(45);
        builder.Property(x => x.ServerGroupId);
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
