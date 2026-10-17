using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Application.Notifications.Rendering;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Infrastructure.Email;
/// <summary>
/// Reads the brand for outgoing mail from the <c>portal.*</c> settings, filling in the
/// storefront's own defaults wherever the operator has set nothing.
/// </summary>
/// <remarks>
/// <para>
/// The logo is the dark-background one, falling back to the light one: the seeded e-mail
/// layout is dark, like the sign-in pages. An uploaded logo is stored as a path relative to
/// the portal (<c>/uploads/branding/…</c>), which a mail client cannot resolve, so it is made
/// absolute against <see cref="ClientPortalOptions.BaseUrl"/> — the portal proxies that path
/// to the API's <c>wwwroot</c>. A pasted absolute URL is left alone.
/// </para>
/// <para>
/// The shades come from <see cref="BrandPalette"/>, the port of the storefront's own ramp, so
/// a button in the inbox matches the button on the site.
/// </para>
/// </remarks>
public sealed class SettingsEmailBrandingProvider : IEmailBrandingProvider
{
    /// <summary>The data context.</summary>
    private readonly AppDbContext _db;

    /// <summary>Where the portal is reachable, for the absolute logo URL.</summary>
    private readonly IOptions<ClientPortalOptions> _clientPortal;

    /// <summary>Initializes a new instance of the <see cref="SettingsEmailBrandingProvider"/> class.</summary>
    /// <param name="db">The data context.</param>
    /// <param name="clientPortal">Where the portal is reachable, for the absolute logo URL.</param>
    public SettingsEmailBrandingProvider(AppDbContext db, IOptions<ClientPortalOptions> clientPortal)
    {
        _db = db;
        _clientPortal = clientPortal;
    }

    /// <inheritdoc />
    public async Task<EmailBranding> GetAsync(CancellationToken ct = default)
    {
        // Was ISettingRepository.ListAsync -- read-only, so no tracking is needed.
        var rows = await _db.Settings.AsNoTracking().ToListAsync(ct);
        string Value(string key) =>
            rows.FirstOrDefault(s => s.Key == key)?.Value?.Trim() ?? string.Empty;

        var primary = BrandPalette.Parse(Value(PortalSettingKeys.BrandPrimary)) is null
            ? EmailBranding.DefaultPrimary
            : Value(PortalSettingKeys.BrandPrimary).ToLowerInvariant();
        var accent = BrandPalette.Parse(Value(PortalSettingKeys.BrandAccent)) is null
            ? EmailBranding.DefaultAccent
            : Value(PortalSettingKeys.BrandAccent).ToLowerInvariant();

        var siteName = Value(PortalSettingKeys.SiteName);
        if (siteName.Length == 0)
        {
            siteName = EmailBranding.DefaultSiteName;
        }

        var logo = Value(PortalSettingKeys.LogoDark);
        if (logo.Length == 0)
        {
            logo = Value(PortalSettingKeys.Logo);
        }

        return new EmailBranding(
            SiteName: siteName,
            LogoUrl: Absolute(logo),
            Primary: primary,
            PrimaryDark: BrandPalette.Shade(primary, 700),
            OnPrimary: BrandPalette.BestTextOn(primary),
            Accent: accent);
    }

    /// <summary>Turns a portal-relative path into an absolute URL; leaves an absolute one, or nothing, alone.</summary>
    /// <param name="url">The stored value.</param>
    /// <returns>An absolute URL or empty.</returns>
    private string Absolute(string url)
    {
        if (url.Length == 0 || Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            return url;
        }

        var baseUrl = _clientPortal.Value.BaseUrl.TrimEnd('/');
        return url.StartsWith('/') ? baseUrl + url : $"{baseUrl}/{url}";
    }
}
