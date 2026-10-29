using System.Reflection;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// The DbContext is the repository (rules/architecture.md): no unit-of-work or repository type
/// sits on top of it, a query handler never saves, and no handler bulk-updates or bulk-deletes
/// around the entity's methods, the interceptors and the domain events. Each rule reads the
/// handler's IL, async state machines and lambdas included.
/// </summary>
public sealed class DbContextUsageTests
{
    /// <summary>The namespace segment every query use case sits under.</summary>
    private const string QueriesSegment = ".Queries.";

    /// <summary>The method names that write straight to the database, skipping the change tracker.</summary>
    private static readonly string[] BulkWrites = ["ExecuteUpdate", "ExecuteUpdateAsync", "ExecuteDelete", "ExecuteDeleteAsync"];

    /// <summary>No layer declares a unit-of-work or repository type; the DbContext is both.</summary>
    [Fact]
    public void NoLayer_DeclaresAUnitOfWorkOrRepository()
    {
        var offenders = new[]
            {
                Layers.SharedKernel, Layers.Domain, Layers.Shared, Layers.Application, Layers.Infrastructure, Layers.Api,
            }
            .SelectMany(Layers.AuthoredTypesOf)
            .Where(type => type.Name.Contains("UnitOfWork", StringComparison.Ordinal)
                || type.Name.EndsWith("Repository", StringComparison.Ordinal))
            .Select(type => type.FullName)
            .ToList();

        offenders.Should().BeEmpty("handlers use IHostpanelDbContext directly and SaveChangesAsync is the unit of work");
    }

    /// <summary>No query handler calls <c>SaveChangesAsync</c>; a query reads and never writes.</summary>
    [Fact]
    public void QueryHandlers_NeverSave()
    {
        var offenders = Handlers()
            .Where(handler => handler.Namespace!.Contains(QueriesSegment, StringComparison.Ordinal))
            .Where(handler => MethodBodyReader.CallsMadeBy(handler).Any(IsSave))
            .Select(handler => handler.FullName)
            .ToList();

        offenders.Should().BeEmpty("a query reads with AsNoTracking and never calls SaveChangesAsync");
    }

    /// <summary>No handler calls <c>ExecuteUpdate</c> or <c>ExecuteDelete</c>.</summary>
    [Fact]
    public void Handlers_NeverBulkWrite()
    {
        var offenders = Handlers()
            .SelectMany(handler => MethodBodyReader.CallsMadeBy(handler)
                .Where(called => BulkWrites.Contains(called.Name))
                .Select(called => $"{handler.FullName} -> {called.Name}"))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        offenders.Should().BeEmpty("a bulk write skips the entity's methods, the interceptors and the domain events");
    }

    /// <summary>The save detector sees a call made inside an async command handler.</summary>
    [Fact]
    public void Detector_SeesSaveInsideAsyncHandlers()
    {
        Handlers()
            .Where(handler => !handler.Namespace!.Contains(QueriesSegment, StringComparison.Ordinal))
            .Should().Contain(handler => MethodBodyReader.CallsMadeBy(handler).Any(IsSave));
    }

    /// <summary>Every concrete use case and event handler in the Application layer.</summary>
    /// <returns>The handler types.</returns>
    private static IEnumerable<Type> Handlers()
    {
        return Layers.AuthoredTypesOf(Layers.Application)
            .Where(type => type.IsClass && !type.IsAbstract && !type.IsNested
                && type.Name.EndsWith("Handler", StringComparison.Ordinal)
                && type.Namespace is not null);
    }

    /// <summary>Whether a called method is a DbContext save.</summary>
    /// <param name="called">The called method.</param>
    /// <returns><c>true</c> for <c>SaveChanges</c> or <c>SaveChangesAsync</c> on any context.</returns>
    private static bool IsSave(MethodBase called)
    {
        return called.Name is "SaveChanges" or "SaveChangesAsync"
            && called.DeclaringType is { } declaring
            && (declaring == typeof(IHostpanelDbContext)
                || typeof(DbContext).IsAssignableFrom(declaring));
    }
}
