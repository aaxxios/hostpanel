using Innovayse.Hostpanel.Domain.Ssh;
using Xunit;

namespace Innovayse.Hostpanel.Domain.Tests.Ssh;

/// <summary>Parsing and fingerprinting of customer-supplied ED25519 public keys.</summary>
public sealed class SshPublicKeyTests
{
    /// <summary>A throwaway key generated with <c>ssh-keygen -t ed25519</c>; its fingerprint is what <c>ssh-keygen -lf</c> printed.</summary>
    private const string RealKey = "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIG6EJIHRxsxge2aZnl+clPBfoX0nmvD7FfHXM2QQ3+3b user@host";

    [Fact]
    public void TryParse_RealKey_FingerprintMatchesSshKeygen()
    {
        Assert.True(SshPublicKey.TryParse(RealKey, out var key, out _));
        Assert.Equal("SHA256:xK0G2nacQ5l2jswa38Dl9jmRCsESwmesIiz26XjE5+o", key!.Fingerprint);
    }

    [Fact]
    public void TryParse_DropsCommentAndSurroundingWhitespace()
    {
        Assert.True(SshPublicKey.TryParse("  " + RealKey + "  \n", out var key, out _));
        Assert.Equal("ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIG6EJIHRxsxge2aZnl+clPBfoX0nmvD7FfHXM2QQ3+3b", key!.Canonical);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ssh-ed25519")]
    [InlineData("ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQ user")]
    [InlineData("ssh-ed25519 AAAAB3NzaC1yc2EAAAADAQABAAABAQ user")] // rsa blob under ed25519 label
    [InlineData("ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIG6EJIHRxsxge2aZnl+clPBfoX0nmvD7FfHXM2QQ3+3b\nssh-ed25519 AAAA second")]
    [InlineData("command=\"rm -rf /\" ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIG6EJIHRxsxge2aZnl+clPBfoX0nmvD7FfHXM2QQ3+3b")]
    [InlineData("ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIG6EJIHRxsxge2aZnl+clPBfoX0nmvD7FfHXM2QQ3+")] // truncated
    public void TryParse_Invalid_Refused(string input)
    {
        Assert.False(SshPublicKey.TryParse(input, out var key, out var isPrivate));
        Assert.Null(key);
        Assert.False(isPrivate);
    }

    [Fact]
    public void TryParse_PrivateKey_FlaggedAsPrivate()
    {
        Assert.False(SshPublicKey.TryParse("-----BEGIN OPENSSH PRIVATE KEY-----\nabc\n-----END OPENSSH PRIVATE KEY-----", out _, out var isPrivate));
        Assert.True(isPrivate);
    }
}
