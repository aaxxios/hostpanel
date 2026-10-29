using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// Dependencies point inward only (rules/architecture.md): Shared never sees a layer above the
/// kernel, Application never sees Infrastructure, the API or a driver, Infrastructure never sees
/// the API, and the request itself stays in the API. Kernel and Domain purity is
/// <see cref="KernelPurityTests"/>.
/// </summary>
public sealed class DependencyDirectionTests
{
    /// <summary>
    /// Assembly name prefixes of drivers and transports the Application layer never references: the
    /// database provider, the cache and the message broker are adapters, reached through a port.
    /// </summary>
    private static readonly string[] DriverPrefixes = ["Npgsql", "StackExchange.Redis", "RabbitMQ"];

    /// <summary>The request-shaped types that never appear in a signature below the API.</summary>
    private static readonly Type[] RequestTypes =
    [
        typeof(HttpContext), typeof(IHttpContextAccessor), typeof(IActionResult), typeof(ClaimsPrincipal), typeof(IFormFile),
    ];

    /// <summary>Shared references the kernel and frameworks only, never a product layer.</summary>
    [Fact]
    public void Shared_ReferencesNoProductLayer()
    {
        AssertReferencesNone(Layers.Shared, Layers.Domain, Layers.Application, Layers.Infrastructure, Layers.Api);
    }

    /// <summary>Application references neither Infrastructure nor the API.</summary>
    [Fact]
    public void Application_ReferencesNeitherInfrastructureNorApi()
    {
        AssertReferencesNone(Layers.Application, Layers.Infrastructure, Layers.Api);
    }

    /// <summary>Application references no database provider, cache client or message broker client.</summary>
    [Fact]
    public void Application_ReferencesNoDriver()
    {
        var offenders = Layers.Application.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .Where(name => DriverPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
            .ToList();

        offenders.Should().BeEmpty("a driver is an adapter behind a port, so it is referenced from Infrastructure only");
    }

    /// <summary>Infrastructure does not reference the API.</summary>
    [Fact]
    public void Infrastructure_DoesNotReferenceTheApi()
    {
        AssertReferencesNone(Layers.Infrastructure, Layers.Api);
    }

    /// <summary>
    /// No Application or Infrastructure type takes, holds or returns the HTTP request, an action
    /// result, a claims principal or an uploaded file; what a handler needs from the request is a port
    /// implemented in the API.
    /// </summary>
    [Fact]
    public void LayersBelowTheApi_DoNotSeeTheRequest()
    {
        var offenders = new[] { Layers.Application, Layers.Infrastructure }
            .SelectMany(Layers.AuthoredTypesOf)
            .SelectMany(type => TypeSignatures.TypesNamedBy(type).Where(IsRequestType).Select(found => $"{type.FullName} -> {found.Name}"))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(line => line, StringComparer.Ordinal)
            .ToList();

        offenders.Should().BeEmpty("the request is read in the API and handed down through a port");
    }

    /// <summary>Asserts an assembly references none of the given layer assemblies.</summary>
    /// <param name="assembly">The assembly under test.</param>
    /// <param name="forbidden">The layers it must not reference.</param>
    private static void AssertReferencesNone(Assembly assembly, params Assembly[] forbidden)
    {
        var forbiddenNames = forbidden.Select(layer => layer.GetName().Name).ToHashSet(StringComparer.Ordinal);
        var offenders = assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => forbiddenNames.Contains(name))
            .ToList();

        offenders.Should().BeEmpty("{0} may only depend inward", assembly.GetName().Name);
    }

    /// <summary>Whether a type is, or derives from, one of the request-shaped types.</summary>
    /// <param name="type">The type.</param>
    /// <returns><c>true</c> when it belongs to the HTTP edge.</returns>
    private static bool IsRequestType(Type type)
    {
        return RequestTypes.Any(request => request.IsAssignableFrom(type));
    }
}
