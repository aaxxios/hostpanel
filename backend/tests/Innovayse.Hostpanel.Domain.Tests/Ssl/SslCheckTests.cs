using Innovayse.Hostpanel.Domain.Ssl;

namespace Innovayse.Hostpanel.Domain.Tests.Ssl;

/// <summary>Unit tests for <see cref="SslCheck"/>.</summary>
public sealed class SslCheckTests
{
    /// <summary>The moment the first check ran.</summary>
    private static readonly DateTimeOffset CheckedAt = new(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);

    /// <summary>Create records the certificate state and stamps when the check ran.</summary>
    [Fact]
    public void Create_RecordsTheCertificateState()
    {
        var expires = new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var check = SslCheck.Create("example.test", hasSSL: true, "Test CA", expires, isActive: true, CheckedAt);

        check.DomainName.Should().Be("example.test");
        check.HasSsl.Should().BeTrue();
        check.Issuer.Should().Be("Test CA");
        check.ExpiresAt.Should().Be(expires);
        check.IsActive.Should().BeTrue();
        check.CheckedAt.Should().Be(CheckedAt);
    }

    /// <summary>A re-check replaces the certificate state and stamps when it ran.</summary>
    [Fact]
    public void RecordCheck_ReplacesTheStateAndRestampsTheCheck()
    {
        var check = SslCheck.Create("example.test", hasSSL: true, "Test CA", CheckedAt.AddDays(30), isActive: true, CheckedAt);
        var recheckedAt = CheckedAt.AddDays(1);

        check.RecordCheck(hasSSL: false, issuer: null, expiresAt: null, isActive: false, recheckedAt);

        check.HasSsl.Should().BeFalse();
        check.Issuer.Should().BeNull();
        check.ExpiresAt.Should().BeNull();
        check.IsActive.Should().BeFalse();
        check.CheckedAt.Should().Be(recheckedAt);
    }
}
