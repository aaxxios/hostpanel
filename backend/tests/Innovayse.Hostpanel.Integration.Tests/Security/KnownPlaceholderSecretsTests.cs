using Innovayse.Hostpanel.Infrastructure.Security;

namespace Innovayse.Hostpanel.Integration.Tests.Security;

/// <summary><see cref="KnownPlaceholderSecrets"/>: template markers and published example values are recognised, real secrets are not.</summary>
public sealed class KnownPlaceholderSecretsTests
{
    /// <summary>Markers, known example values and base64 of marker text are placeholders, whatever their case.</summary>
    /// <param name="value">The candidate secret.</param>
    [Theory]
    [InlineData("CHANGE_ME")]
    [InlineData("change_me_please")]
    [InlineData("  CHANGE_ME_X  ")]
    [InlineData("changeme")]
    [InlineData("Postgres")]
    [InlineData("guest")]
    [InlineData("Q0hBTkdFX01FX0NIQU5HRV9NRV9DSEFOR0VfTUVfMDA=")]
    public void Placeholders_AreRecognised(string value)
    {
        KnownPlaceholderSecrets.IsPlaceholder(value).Should().BeTrue();
    }

    /// <summary>Generated values are not placeholders, hex or base64.</summary>
    /// <param name="value">The candidate secret.</param>
    [Theory]
    [InlineData("4f8a1c7e92d04b36a5e8c1d7f3b29a60e4d8c7b1a2f3e5d6c9b8a7f6e5d4c3b2")]
    [InlineData("MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY=")]
    [InlineData("k3Jx9Qm2")]
    public void GeneratedValues_AreNotPlaceholders(string value)
    {
        KnownPlaceholderSecrets.IsPlaceholder(value).Should().BeFalse();
    }
}
