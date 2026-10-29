using Innovayse.Hostpanel.Domain.Settings;

namespace Innovayse.Hostpanel.Domain.Tests.Settings;

/// <summary>Unit tests for the <see cref="Setting"/> aggregate.</summary>
public sealed class SettingTests
{
    /// <summary>Create keeps the key, value and description.</summary>
    [Fact]
    public void Create_KeepsFields()
    {
        var setting = Setting.Create("site:name", "Panel", "Site name.");

        setting.Key.Should().Be("site:name");
        setting.Value.Should().Be("Panel");
        setting.Description.Should().Be("Site name.");
    }

    /// <summary>UpdateValue replaces the value and leaves the key alone.</summary>
    [Fact]
    public void UpdateValue_ReplacesValue()
    {
        var setting = Setting.Create("site:name", "Panel", null);

        setting.UpdateValue("New panel");

        setting.Value.Should().Be("New panel");
        setting.Key.Should().Be("site:name");
    }
}
