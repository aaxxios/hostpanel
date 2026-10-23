using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Admin.Integrations.Common;
using Innovayse.Hostpanel.Domain.Settings;
using Xunit;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// Holds <see cref="SecretSettingKeys"/> as the single source of which settings are secret: the
/// integration catalogue's password fields and every shipped plugin manifest's secret fields must
/// agree with it, so a credential cannot be rendered as a password box yet returned by a settings
/// read, or the other way round.
/// </summary>
public sealed class SecretSettingClassificationTests
{
    /// <summary>A catalogue field is a password field exactly when the registry classifies it as secret.</summary>
    [Fact]
    public void Catalogue_PasswordFields_MatchTheRegistry()
    {
        foreach (var (slug, entry) in IntegrationCatalogue.All)
        {
            foreach (var field in entry.Fields)
            {
                var key = IntegrationSettingKeys.FieldKey(slug, field.Key);
                SecretSettingKeys.IsSecret(key).Should().Be(
                    SecretSettingKeys.IsSecretFieldType(field.Type),
                    "{0} is typed '{1}' in the catalogue",
                    key,
                    field.Type);
            }
        }
    }

    /// <summary>Every field a shipped plugin manifest declares as secret is in the registry too.</summary>
    [Fact]
    public void ShippedPluginManifests_SecretFields_AreInTheRegistry()
    {
        var manifests = Directory.GetFiles(SourceRoot(), "plugin.json", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToList();

        manifests.Should().NotBeEmpty();
        foreach (var path in manifests)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            foreach (var field in document.RootElement.GetProperty("fields").EnumerateArray())
            {
                var name = field.GetProperty("key").GetString()!;
                var type = field.GetProperty("type").GetString();
                SecretSettingKeys.IntegrationSecretFields.Contains(name).Should().Be(
                    SecretSettingKeys.IsSecretFieldType(type),
                    "{0} declares '{1}' as '{2}'",
                    path,
                    name,
                    type);
            }
        }
    }

    /// <summary>Finds <c>backend/src</c> by walking up from the test binaries.</summary>
    /// <returns>The absolute path of the source root.</returns>
    private static string SourceRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "Innovayse.SDK");
            if (Directory.Exists(candidate))
            {
                return Path.Combine(directory.FullName, "src");
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("backend/src was not found above the test binaries.");
    }
}
