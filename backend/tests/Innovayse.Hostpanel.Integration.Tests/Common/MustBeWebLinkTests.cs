namespace Innovayse.Hostpanel.Integration.Tests.Common;

/// <summary>Pins the link rule the storefront content validators share: only addresses safe to render pass.</summary>
public sealed class MustBeWebLinkTests
{
    /// <summary>Message the probe reports for a refused link.</summary>
    private const string LinkMessage = WebLinkProbeValidator.Message;

    /// <summary>Absent, empty, root-relative and http(s) links pass.</summary>
    /// <param name="link">The link under test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/uploads/branding/content/a.png")]
    [InlineData("https://example.com/path?x=1")]
    [InlineData("http://example.com")]
    public void SafeLinks_Pass(string? link)
    {
        new WebLinkProbeValidator().Validate(new WebLinkProbe(link)).IsValid.Should().BeTrue();
    }

    /// <summary>Script, data, protocol-relative and scheme-less values are refused with the supplied message.</summary>
    /// <param name="link">The link under test.</param>
    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html;base64,AAAA")]
    [InlineData("//evil.example/x")]
    [InlineData("/\\evil.example")]
    [InlineData("example.com")]
    [InlineData("ftp://example.com/file")]
    public void UnsafeLinks_AreRefused(string link)
    {
        var result = new WebLinkProbeValidator().Validate(new WebLinkProbe(link));

        result.Errors.Should().ContainSingle().Which.ErrorMessage.Should().Be(LinkMessage);
    }
}
