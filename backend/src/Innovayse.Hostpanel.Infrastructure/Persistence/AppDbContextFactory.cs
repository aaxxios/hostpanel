using Innovayse.Hostpanel.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Innovayse.Hostpanel.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by EF Core tooling (dotnet ef migrations) to create
/// an <see cref="AppDbContext"/> without starting the full application host.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>
    /// Creates an <see cref="AppDbContext"/> configured with a placeholder connection string
    /// suitable for generating migrations at design time.
    /// </summary>
    /// <remarks>
    /// The context is given a <see cref="ModelOnlyEncryptionService"/>: the tooling only needs the
    /// model, and that service holds no key and refuses to encrypt or decrypt, so a design-time
    /// context can never write a protected column with a made-up key or in plain text.
    /// </remarks>
    /// <param name="args">Command-line arguments passed by the tooling (unused).</param>
    /// <returns>A configured <see cref="AppDbContext"/> instance.</returns>
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                "Host=postgres;Database=innovayse_dev;Username=postgres;Password=postgres",
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .Options;

        return new AppDbContext(options, new ModelOnlyEncryptionService());
    }
}
