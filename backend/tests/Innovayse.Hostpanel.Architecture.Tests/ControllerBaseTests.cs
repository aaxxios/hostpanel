using FluentAssertions;
using Innovayse.Hostpanel.Shared.Controllers;
using Xunit;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// Every controller derives from <see cref="BaseApiController"/>, the one place a result becomes a status
/// and an error body.
/// </summary>
public sealed class ControllerBaseTests
{
    /// <summary>
    /// Controllers that still derive straight from <c>ControllerBase</c>. <c>LocalAuthController</c> calls
    /// the user service directly and returns identity-provider shapes (see its remarks).
    /// </summary>
    private static readonly HashSet<string> AllowedDirectDerivations =
    [
        "Innovayse.Hostpanel.API.Auth.LocalAuthController",
    ];

    /// <summary>Every non-abstract controller outside the allow-list is a <see cref="BaseApiController"/>.</summary>
    [Fact]
    public void Controllers_DeriveFromBaseApiController()
    {
        var offenders = Layers.AuthoredTypesOf(Layers.Api)
            .Where(type => type.IsClass && !type.IsAbstract
                && type.Name.EndsWith("Controller", StringComparison.Ordinal))
            .Where(type => !typeof(BaseApiController).IsAssignableFrom(type))
            .Select(type => type.FullName!)
            .Where(name => !AllowedDirectDerivations.Contains(name))
            .ToList();

        offenders.Should().BeEmpty("derive from BaseApiController and return through HandleResult");
    }
}
