using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Servers.Enums;

namespace Innovayse.Hostpanel.Domain.Tests.Servers;

/// <summary>Unit tests for <see cref="ServerGroup"/>.</summary>
public sealed class ServerGroupTests
{
    /// <summary>Create sets the name and fill type and starts with no servers.</summary>
    [Fact]
    public void Create_SetsNameAndFillType()
    {
        var group = ServerGroup.Create("Shared", ServerFillType.LeastFull);

        group.Name.Should().Be("Shared");
        group.FillType.Should().Be(ServerFillType.LeastFull);
        group.Servers.Should().BeEmpty();
    }

    /// <summary>Rename changes the name and keeps the fill type.</summary>
    [Fact]
    public void Rename_ChangesOnlyTheName()
    {
        var group = ServerGroup.Create("Shared", ServerFillType.LeastFull);

        group.Rename("Dedicated");

        group.Name.Should().Be("Dedicated");
        group.FillType.Should().Be(ServerFillType.LeastFull);
    }

    /// <summary>ChangeFillType changes the strategy and keeps the name.</summary>
    [Fact]
    public void ChangeFillType_ChangesOnlyTheStrategy()
    {
        var group = ServerGroup.Create("Shared", ServerFillType.LeastFull);

        group.ChangeFillType(ServerFillType.FillUntilFull);

        group.Name.Should().Be("Shared");
        group.FillType.Should().Be(ServerFillType.FillUntilFull);
    }
}
