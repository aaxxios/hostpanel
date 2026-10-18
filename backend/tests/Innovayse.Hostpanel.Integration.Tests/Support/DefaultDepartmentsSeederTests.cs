using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Seeding;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// Tests for <see cref="DefaultDepartmentsSeeder"/> against Postgres: it creates the standard
/// departments once on an empty table and never touches one an operator has already changed.
/// </summary>
/// <remarks>
/// Moved onto <see cref="IHostpanelDbContext"/> in A6 Task 6 -- the seeder previously took
/// <c>IDepartmentRepository</c>, now dissolved. <c>Program.cs</c> skips this seeder entirely in the
/// <c>Testing</c> environment (migrations and seeding there are the test factory's own job), so
/// nothing before this file ever exercised it against a real database. This class does not share
/// its Postgres container with any other test class (no <c>ICollectionFixture</c> in this suite),
/// so clearing the <c>departments</c> table here cannot affect another class's assertions.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DefaultDepartmentsSeederTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Deletes every row in the <c>departments</c> table so a test starts from empty.</summary>
    /// <param name="context">The data context to clear.</param>
    private static async Task ClearDepartmentsAsync(IHostpanelDbContext context)
    {
        await context.Departments.ExecuteDeleteAsync();
    }

    /// <summary>An empty table gets every default department created exactly once.</summary>
    [Fact]
    public async Task EnsureSeededAsync_EmptyTable_CreatesEveryDefaultOnceAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await ClearDepartmentsAsync(context);

        var created = await DefaultDepartmentsSeeder.EnsureSeededAsync(context, uow);

        Assert.Equal(4, created);
        var names = await context.Departments.AsNoTracking().Select(d => d.Name).ToListAsync();
        Assert.Equal(
            new[] { "Technical Support", "Billing", "Sales", "General" }.OrderBy(n => n),
            names.OrderBy(n => n));
    }

    /// <summary>
    /// Nothing is created, and an operator's own department survives untouched, when the table
    /// already holds at least one row -- the seeder is keyed on "the table is empty", not on names.
    /// </summary>
    [Fact]
    public async Task EnsureSeededAsync_TableAlreadyHasARow_CreatesNothingAndLeavesItAloneAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await ClearDepartmentsAsync(context);
        context.Departments.Add(Department.Create("Renamed Support Desk", "renamed@example.test"));
        await context.SaveChangesAsync(CancellationToken.None);

        var created = await DefaultDepartmentsSeeder.EnsureSeededAsync(context, uow);

        Assert.Equal(0, created);
        var department = await context.Departments.AsNoTracking().SingleAsync();
        Assert.Equal("Renamed Support Desk", department.Name);
    }
}
