using System.ComponentModel.DataAnnotations;

namespace Innovayse.Hostpanel.Shared.Settings;

/// <summary>How long a caller is told to wait before retrying a request answered with 503.</summary>
public sealed class ServiceUnavailableOptions
{
    /// <summary>The configuration section these options bind from.</summary>
    public const string SectionName = "ServiceUnavailable";

    /// <summary>The <c>Retry-After</c> value, in seconds, sent on every 503.</summary>
    [Range(1, 3600)]
    public int RetryAfterSeconds { get; init; } = 30;
}
