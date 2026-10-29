using Innovayse.Hostpanel.Domain.Plugins;

namespace Innovayse.Hostpanel.Domain.Tests.Plugins;

/// <summary>The plugin id rule: plain folder-safe names pass, anything a path could read as structure fails.</summary>
public sealed class PluginIdentifierTests
{
    /// <summary>Lower-case words joined by single hyphens are accepted.</summary>
    /// <param name="id">The candidate id.</param>
    [Theory]
    [InlineData("inecobank")]
    [InlineData("test-plugin-0123abcd")]
    [InlineData("a")]
    [InlineData("pay2-go")]
    public void AcceptsPlainIds(string id)
    {
        PluginIdentifier.IsValid(id).Should().BeTrue();
    }

    /// <summary>Empty values, separators, parent references, absolute paths and odd casing are refused.</summary>
    /// <param name="id">The candidate id.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("../outside")]
    [InlineData("a/b")]
    [InlineData("a\\b")]
    [InlineData("/etc")]
    [InlineData("C:")]
    [InlineData("Upper")]
    [InlineData("-leading")]
    [InlineData("trailing-")]
    [InlineData("double--hyphen")]
    [InlineData("with space")]
    [InlineData("dot.name")]
    public void RefusesUnsafeIds(string? id)
    {
        PluginIdentifier.IsValid(id).Should().BeFalse();
    }

    /// <summary>An id longer than <see cref="PluginIdentifier.MaxLength"/> is refused.</summary>
    [Fact]
    public void RefusesOverlongIds()
    {
        PluginIdentifier.IsValid(new string('a', PluginIdentifier.MaxLength)).Should().BeTrue();
        PluginIdentifier.IsValid(new string('a', PluginIdentifier.MaxLength + 1)).Should().BeFalse();
    }
}
