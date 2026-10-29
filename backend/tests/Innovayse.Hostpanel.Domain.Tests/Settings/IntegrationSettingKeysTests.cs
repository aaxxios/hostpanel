using Innovayse.Hostpanel.Domain.Settings;

namespace Innovayse.Hostpanel.Domain.Tests.Settings;

/// <summary>Unit tests for the key builders on <see cref="IntegrationSettingKeys"/>.</summary>
public sealed class IntegrationSettingKeysTests
{
    /// <summary>Prefix scopes a slug under the integration root.</summary>
    [Fact]
    public void Prefix_ScopesTheSlugUnderTheRoot()
    {
        IntegrationSettingKeys.Prefix("mailer").Should().Be($"{IntegrationSettingKeys.RootPrefix}mailer:");
    }

    /// <summary>Section is the prefix without its trailing separator, the path a configuration section is read under.</summary>
    [Fact]
    public void Section_IsThePrefixWithoutTheSeparator()
    {
        IntegrationSettingKeys.Section("mailer").Should().Be($"{IntegrationSettingKeys.RootPrefix}mailer");
        IntegrationSettingKeys.Prefix("mailer").Should().Be($"{IntegrationSettingKeys.Section("mailer")}:");
    }

    /// <summary>FieldKey appends the field to the slug's prefix.</summary>
    [Fact]
    public void FieldKey_AppendsTheField()
    {
        IntegrationSettingKeys.FieldKey("mailer", "host")
            .Should().Be($"{IntegrationSettingKeys.Prefix("mailer")}host");
    }

    /// <summary>EnabledKey is the field key of the enabled flag.</summary>
    [Fact]
    public void EnabledKey_IsTheEnabledFieldKey()
    {
        IntegrationSettingKeys.EnabledKey("mailer")
            .Should().Be(IntegrationSettingKeys.FieldKey("mailer", IntegrationSettingKeys.IsEnabledField));
    }
}
