using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Common;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An <see cref="IEmailBrandingProvider"/> standing in for the real <c>portal.*</c> settings
/// lookup: it answers whatever <see cref="BrandToReturn"/> was set to before the render, never
/// touching the database. A fake port, not a mock of our own code.
/// </summary>
public sealed class FakeEmailBrandingProvider : IEmailBrandingProvider
{
    /// <summary>The brand <see cref="GetAsync"/> answers with.</summary>
    public EmailBranding BrandToReturn { get; set; } = new(
        SiteName: "Acme Hosting",
        LogoUrl: "https://acme.test/uploads/branding/logodark/x/logo.png",
        Primary: "#1a73e8",
        PrimaryDark: "#154485",
        OnPrimary: "#ffffff",
        Accent: "#e8710a");

    /// <inheritdoc/>
    public Task<EmailBranding> GetAsync(CancellationToken ct = default) => Task.FromResult(BrandToReturn);
}
