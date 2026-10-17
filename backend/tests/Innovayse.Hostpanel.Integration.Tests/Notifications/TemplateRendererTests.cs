using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Application.Notifications.Rendering;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Notifications;

/// <summary>
/// Tests for <see cref="TemplateRenderer"/>: the caller's model renders as before, the brand
/// and site objects are there for every template, and the caller's own member wins on a clash.
/// </summary>
/// <remarks>
/// Branding comes from <see cref="FakeEmailBrandingProvider"/> — <c>IEmailBrandingProvider</c> is a
/// port this product owns, and the rule is no <c>Moq</c> of our own code.
/// </remarks>
public sealed class TemplateRendererTests
{
    /// <summary>The branding the renderer under test reads from, unless overridden per-call.</summary>
    private readonly FakeEmailBrandingProvider _branding = new();

    /// <summary>The brand every test starts from.</summary>
    private static readonly EmailBranding Acme = new(
        SiteName: "Acme Hosting",
        LogoUrl: "https://acme.test/uploads/branding/logodark/x/logo.png",
        Primary: "#1a73e8",
        PrimaryDark: "#154485",
        OnPrimary: "#ffffff",
        Accent: "#e8710a");

    /// <summary>Builds the renderer under test, over the given brand.</summary>
    /// <param name="brand">The brand the fake provider answers with, defaulting to <see cref="Acme"/>.</param>
    /// <returns>The renderer.</returns>
    private TemplateRenderer CreateRenderer(EmailBranding? brand = null)
    {
        _branding.BrandToReturn = brand ?? Acme;
        return new TemplateRenderer(_branding);
    }

    /// <summary>The caller's own model renders exactly as before the brand/site injection existed.</summary>
    [Fact]
    public async Task RenderAsync_CallerModel_RendersAsBefore()
    {
        var html = await CreateRenderer().RenderAsync("Reset: {{ reset_link }}", new { reset_link = "https://x/reset?t=1" });

        Assert.Equal("Reset: https://x/reset?t=1", html);
    }

    /// <summary>Every field of the brand and the site name are available beside the caller's model.</summary>
    [Fact]
    public async Task RenderAsync_ExposesBrandAndSiteBesideTheModel()
    {
        var html = await CreateRenderer().RenderAsync(
            "{{ site.name }}|{{ brand.primary }}|{{ brand.primary_dark }}|{{ brand.on_primary }}|{{ brand.accent }}|{{ brand.logo_url }}|{{ reset_link }}",
            new { reset_link = "L" });

        Assert.Equal("Acme Hosting|#1a73e8|#154485|#ffffff|#e8710a|https://acme.test/uploads/branding/logodark/x/logo.png|L", html);
    }

    /// <summary>A logo-or-site-name branch in a seeded layout template follows whichever brand is injected.</summary>
    [Fact]
    public async Task RenderAsync_LogoBranchInTheSeededLayout_FollowsTheBrand()
    {
        const string template = "{% if brand.logo_url != \"\" %}<img src=\"{{ brand.logo_url }}\">{% else %}<span>{{ site.name }}</span>{% endif %}";

        var withLogo = await CreateRenderer().RenderAsync(template, new { });
        var without = await CreateRenderer(Acme with { LogoUrl = "" }).RenderAsync(template, new { });

        Assert.Contains("<img src=\"https://acme.test", withLogo);
        Assert.Equal("<span>Acme Hosting</span>", without);
    }

    /// <summary>A model member named <c>brand</c> wins over the injected brand object.</summary>
    [Fact]
    public async Task RenderAsync_CallerOwnBrandMember_WinsOverTheInjectedOne()
    {
        var html = await CreateRenderer().RenderAsync(
            "{{ brand }}|{{ site.name }}",
            new { brand = "caller's brand" });

        Assert.Equal("caller's brand|Acme Hosting", html);
    }

    /// <summary>A dictionary model's own <c>site</c> key wins over the injected site object.</summary>
    [Fact]
    public async Task RenderAsync_DictionaryModel_KeepsItsOwnSiteKey()
    {
        var model = new Dictionary<string, object> { ["site"] = "caller's site", ["x"] = 1 };

        var html = await CreateRenderer().RenderAsync("{{ site }}|{{ brand.primary }}", model);

        Assert.Equal("caller's site|#1a73e8", html);
    }

    /// <summary>A differently-cased key in the model does not suppress the injected object.</summary>
    [Fact]
    public async Task RenderAsync_DifferentlyCasedKey_DoesNotSuppressTheInjectedObject()
    {
        // Fluid resolves names exactly, so "Site" is not {{ site }} and the brand's site must
        // still be there.
        var model = new Dictionary<string, object> { ["Site"] = "caller's site" };

        var html = await CreateRenderer().RenderAsync("{{ site.name }}", model);

        Assert.Equal("Acme Hosting", html);
    }

    /// <summary>A template the parser cannot read throws rather than rendering partial output.</summary>
    [Fact]
    public async Task RenderAsync_ParseError_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateRenderer().RenderAsync("{{ unclosed", new { }));
    }
}
