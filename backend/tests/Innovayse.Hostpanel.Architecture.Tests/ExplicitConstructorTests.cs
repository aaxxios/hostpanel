using System.Reflection;
using FluentAssertions;
using Xunit;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// Handlers and controllers declare <c>private readonly</c> fields and a written constructor; a primary
/// constructor hides which dependencies a class holds (rules/cqrs.md, rules/controllers.md).
/// </summary>
public sealed class ExplicitConstructorTests
{
    /// <summary>
    /// Controllers still on a primary constructor, recorded rather than fixed here. Remove the entry when
    /// the controller is converted.
    /// </summary>
    private static readonly HashSet<string> PrimaryConstructorControllers =
    [
        "Innovayse.Hostpanel.API.Auth.LocalAuthController",
    ];

    /// <summary>No handler in the Application layer captures a primary-constructor parameter.</summary>
    [Fact]
    public void Handlers_UseExplicitConstructors()
    {
        var offenders = Layers.AuthoredTypesOf(Layers.Application)
            .Where(type => type.IsClass && !type.IsAbstract
                && type.Name.EndsWith("Handler", StringComparison.Ordinal))
            .Where(CapturesPrimaryConstructorParameters)
            .Select(type => type.FullName)
            .ToList();

        offenders.Should().BeEmpty("write the field and the constructor");
    }

    /// <summary>No controller outside the allow-list captures a primary-constructor parameter.</summary>
    [Fact]
    public void Controllers_UseExplicitConstructors()
    {
        var offenders = Layers.AuthoredTypesOf(Layers.Api)
            .Where(type => type.IsClass && !type.IsAbstract
                && type.Name.EndsWith("Controller", StringComparison.Ordinal))
            .Where(CapturesPrimaryConstructorParameters)
            .Select(type => type.FullName!)
            .Where(name => !PrimaryConstructorControllers.Contains(name))
            .ToList();

        offenders.Should().BeEmpty("write the field and the constructor");
    }

    /// <summary>
    /// The compiler stores a captured primary-constructor parameter in a field named <c>&lt;name&gt;P</c>;
    /// an explicit field can never carry that name, so its presence identifies the syntax.
    /// </summary>
    /// <param name="type">The class to inspect.</param>
    /// <returns><c>true</c> when the class captures a primary-constructor parameter.</returns>
    private static bool CapturesPrimaryConstructorParameters(Type type)
    {
        return type
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Any(field => field.Name.StartsWith("<", StringComparison.Ordinal)
                && field.Name.EndsWith(">P", StringComparison.Ordinal));
    }
}
