using Innovayse.Hostpanel.Infrastructure.Security.Settings;
using Microsoft.Extensions.Hosting.Internal;

namespace Innovayse.Hostpanel.Integration.Tests.Security;

/// <summary>
/// <see cref="DeployedSecretsValidator"/>: a deployed tier refuses template placeholders, the
/// published example values and secrets that are too short, naming each offending key; Development and
/// the test host are exempt.
/// </summary>
public sealed class DeployedSecretsValidatorTests
{
    /// <summary>A real-looking base64 key of 32 bytes (the ASCII text "0123456789abcdef0123456789abcdef").</summary>
    private const string GoodEncryptionKey = "MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY=";

    /// <summary>A signing secret of 64 hex characters.</summary>
    private const string GoodJwtSecret = "4f8a1c7e92d04b36a5e8c1d7f3b29a60e4d8c7b1a2f3e5d6c9b8a7f6e5d4c3b2";

    /// <summary>A sign-out key of 48 hex characters.</summary>
    private const string GoodBackchannelKey = "9b2f7a41c6e83d05f1a7b2c4d6e8f0a1b3c5d7e9f1a2b3c4";

    /// <summary>A database password of 48 hex characters.</summary>
    private const string GoodDatabasePassword = "a1b2c3d4e5f60718293a4b5c6d7e8f90a1b2c3d4e5f60718";

    /// <summary>Builds a validator for the named tier.</summary>
    /// <param name="environmentName">The hosting environment name.</param>
    /// <returns>The validator under test.</returns>
    private static DeployedSecretsValidator For(string environmentName)
    {
        return new DeployedSecretsValidator(new HostingEnvironment { EnvironmentName = environmentName });
    }

    /// <summary>A complete set of strong secrets.</summary>
    /// <returns>The options.</returns>
    private static DeployedSecrets Strong() => new()
    {
        JwtSecret = GoodJwtSecret,
        EncryptionKey = GoodEncryptionKey,
        BackchannelLogoutKey = GoodBackchannelKey,
        DatabasePassword = GoodDatabasePassword,
    };

    /// <summary>Strong secrets pass on a deployed tier.</summary>
    [Fact]
    public void Production_WithStrongSecrets_IsValid()
    {
        For("Production").Validate(null, Strong()).Succeeded.Should().BeTrue();
    }

    /// <summary>The optional secrets may be absent: a host with no sign-out key or no database password is not refused for it.</summary>
    [Fact]
    public void Production_WithoutTheOptionalSecrets_IsValid()
    {
        var options = Strong();
        options.BackchannelLogoutKey = string.Empty;
        options.DatabasePassword = null;

        For("Production").Validate(null, options).Succeeded.Should().BeTrue();
    }

    /// <summary>Development and the test host keep their template values.</summary>
    /// <param name="environmentName">The exempt tier.</param>
    [Theory]
    [InlineData("Development")]
    [InlineData("Testing")]
    public void ExemptTiers_AcceptPlaceholders(string environmentName)
    {
        var options = new DeployedSecrets { JwtSecret = "CHANGE_ME", EncryptionKey = "CHANGE_ME" };

        For(environmentName).Validate(null, options).Succeeded.Should().BeTrue();
    }

    /// <summary>A template placeholder or a published example value is refused for every secret, naming the key.</summary>
    /// <param name="secret">Which secret carries the bad value.</param>
    /// <param name="value">The bad value.</param>
    /// <param name="expectedKey">A fragment of the key the failure must name.</param>
    [Theory]
    [InlineData("jwt", "CHANGE_ME_generate_with_openssl_rand_hex_32_chars", "Jwt:Secret")]
    [InlineData("jwt", "change-this-to-a-32-char-min-secret-key-here", "Jwt:Secret")]
    [InlineData("encryption", "Q0hBTkdFX01FX0NIQU5HRV9NRV9DSEFOR0VfTUVfMDA=", "EncryptionKey")]
    [InlineData("encryption", "ZGV2LW9ubHkta2V5LW5vdC1mb3ItcHJvZHVjdGlvbiE=", "EncryptionKey")]
    [InlineData("backchannel", "dev-hostpanel-backchannel-key", "BackchannelLogout:Key")]
    [InlineData("database", "postgres", "database password")]
    [InlineData("database", "CHANGE_ME", "database password")]
    public void Production_WithAPlaceholder_IsRefused(string secret, string value, string expectedKey)
    {
        var options = Strong();
        switch (secret)
        {
            case "jwt":
                options.JwtSecret = value;
                break;
            case "encryption":
                options.EncryptionKey = value;
                break;
            case "backchannel":
                options.BackchannelLogoutKey = value;
                break;
            default:
                options.DatabasePassword = value;
                break;
        }

        var result = For("Production").Validate(null, options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain(expectedKey).And.Contain("placeholder");
    }

    /// <summary>A secret below its minimum length is refused, naming the key and the length.</summary>
    [Fact]
    public void Production_WithAShortSecret_IsRefused()
    {
        var options = Strong();
        options.JwtSecret = "short-but-not-a-placeholder";
        options.BackchannelLogoutKey = "tooshort";
        options.DatabasePassword = "hunter2";

        var result = For("Production").Validate(null, options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should()
            .Contain($"shorter than {DeployedSecretsValidator.MinimumJwtSecretLength}")
            .And.Contain($"shorter than {DeployedSecretsValidator.MinimumBackchannelKeyLength}")
            .And.Contain($"shorter than {DeployedSecretsValidator.MinimumDatabasePasswordLength}");
    }

    /// <summary>The two secrets every host needs are refused when missing.</summary>
    [Fact]
    public void Production_WithoutTheRequiredSecrets_IsRefused()
    {
        var options = new DeployedSecrets { EncryptionKey = " ", JwtSecret = null };

        var result = For("Production").Validate(null, options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Jwt:Secret").And.Contain("EncryptionKey").And.Contain("is not set");
    }

    /// <summary>Every offending key is named in one failure, so an operator fixes them in one pass.</summary>
    [Fact]
    public void Production_NamesEveryOffendingKey()
    {
        var options = new DeployedSecrets
        {
            JwtSecret = "CHANGE_ME",
            EncryptionKey = "CHANGE_ME",
            BackchannelLogoutKey = "CHANGE_ME",
            DatabasePassword = "CHANGE_ME",
        };

        var result = For("Production").Validate(null, options);

        result.Failures.Should().HaveCount(4);
    }
}
