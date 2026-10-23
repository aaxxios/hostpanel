using Innovayse.Hostpanel.Domain.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;
/// <summary>EF Core configuration for the <see cref="Department"/> entity.</summary>
public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    /// <summary>Configures the <c>Departments</c> table mapping.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
