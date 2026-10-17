using FluentAssertions;
using Xunit;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// An interface the Application layer owns is a port, and ports live in one folder so a reader finds
/// every seam to the outside in one listing (rules/file-layout.md).
/// </summary>
public sealed class PortPlacementTests
{
    /// <summary>The only namespace in which the Application layer may declare an interface.</summary>
    private const string PortNamespace = "Innovayse.Hostpanel.Application.Common.Interfaces";

    /// <summary>Every top-level interface in the Application assembly sits in <c>Common/Interfaces</c>.</summary>
    [Fact]
    public void ApplicationInterfaces_LiveOnlyInCommonInterfaces()
    {
        var offenders = Layers.AuthoredTypesOf(Layers.Application)
            .Where(type => type.IsInterface && !type.IsNested && type.Namespace != PortNamespace)
            .Select(type => type.FullName)
            .ToList();

        offenders.Should().BeEmpty("ports belong in Application/Common/Interfaces");
    }
}
