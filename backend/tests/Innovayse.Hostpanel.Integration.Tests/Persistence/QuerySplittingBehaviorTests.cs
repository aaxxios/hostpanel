using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Persistence;

/// <summary>
/// Pins the composition root's EF Core query-splitting default.
/// </summary>
/// <remarks>
/// Some queries load more than one collection navigation at once, and EF Core's default
/// is one SQL statement whose row count is the product of those collections. EF logs a
/// warning about it on every such run. Splitting is the default set in
/// <c>Infrastructure/DependencyInjection.cs</c>, and this test notices if that line is lost
/// from the options builder.
/// </remarks>
public sealed class QuerySplittingBehaviorTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The registered <see cref="AppDbContext"/> options split multi-collection queries.</summary>
    [Fact]
    public void DbContextOptions_SplitQueriesByDefault()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<AppDbContext>>();

        // Act
        var relational = options.Extensions.OfType<RelationalOptionsExtension>().Single();

        // Assert
        relational.QuerySplittingBehavior.Should().Be(QuerySplittingBehavior.SplitQuery);
    }
}
