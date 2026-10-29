namespace Innovayse.Hostpanel.Application.Common.Settings;

/// <summary>
/// Where the admin panel is reachable, for the links this backend puts into mail addressed to
/// staff.
/// </summary>
/// <remarks>
/// Not a section: <c>AdminBaseUrl</c> is a single bare top-level key, like <c>ClientBaseUrl</c>
/// (<see cref="ClientPortalOptions"/>), so the class names the key it is built from.
/// </remarks>
public sealed class AdminPanelOptions
{
    /// <summary>The configuration key this value is read from. Not a section: a bare top-level key.</summary>
    public const string ConfigurationKey = "AdminBaseUrl";

    /// <summary>
    /// Absolute base URL of the admin panel, scheme included and no trailing slash: paths such as
    /// <c>/verify-email</c> are appended to it. The default is the admin dev server on a developer
    /// machine, so a deployed tier must override it or its staff mails point at localhost.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:5174";
}
