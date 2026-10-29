using Innovayse.Hostpanel.Shared.Controllers;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// Every controller derives from <see cref="BaseApiController"/>, the one place a result becomes a status
/// and an error body.
/// </summary>
public sealed class ControllerBaseTests
{
    /// <summary>Every non-abstract controller is a <see cref="BaseApiController"/>.</summary>
    [Fact]
    public void Controllers_DeriveFromBaseApiController()
    {
        var offenders = Layers.AuthoredTypesOf(Layers.Api)
            .Where(type => type.IsClass && !type.IsAbstract
                && type.Name.EndsWith("Controller", StringComparison.Ordinal))
            .Where(type => !typeof(BaseApiController).IsAssignableFrom(type))
            .Select(type => type.FullName!)
            .ToList();

        offenders.Should().BeEmpty("derive from BaseApiController and return through HandleResult");
    }
}
