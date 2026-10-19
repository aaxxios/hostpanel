namespace Innovayse.Hostpanel.Shared.Constants;

/// <summary>Request-size ceilings shared by the controllers that accept an uploaded picture.</summary>
public static class BrandingUploadLimits
{
    /// <summary>
    /// The ceiling the framework enforces before it buffers an upload at all: 6 MiB, deliberately above the
    /// operator-facing <c>Branding:MaxBytes</c> (5 MiB), which answers with a readable refusal.
    /// </summary>
    public const long RequestSizeLimitBytes = 6 * 1024 * 1024;
}
