using System.Diagnostics.Metrics;

namespace Innovayse.Hostpanel.Shared.Infrastructure.Observability;

/// <summary>
/// Counts requests the edge rate limiter rejected, tagged by policy and client type, so a flood
/// or a mis-sized limit shows on a dashboard and not only in logs.
/// </summary>
public sealed class RateLimitMetrics
{
    /// <summary>The meter name exporters subscribe to.</summary>
    public const string MeterName = "Innovayse.Hostpanel.RateLimiting";

    /// <summary>The instrument name of the rejection counter.</summary>
    public const string RejectionsCounterName = "hostpanel.rate_limit.rejections";

    /// <summary>The rejection counter.</summary>
    private readonly Counter<long> _rejections;

    /// <summary>Creates the meter through the host's factory so its lifetime follows the container.</summary>
    /// <param name="meterFactory">The host's meter factory.</param>
    public RateLimitMetrics(IMeterFactory meterFactory)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);
        var meter = meterFactory.Create(MeterName);
        _rejections = meter.CreateCounter<long>(
            RejectionsCounterName, "{request}", "Requests rejected by the edge rate limiter.");
    }

    /// <summary>Records one rejected request.</summary>
    /// <param name="policy">The policy that rejected it: global, auth, api or concurrent.</param>
    /// <param name="clientType">Whether the client was identified as a <c>user</c> or an <c>ip</c>.</param>
    public void RecordRejection(string policy, string clientType)
    {
        _rejections.Add(
            1,
            new KeyValuePair<string, object?>("policy", policy),
            new KeyValuePair<string, object?>("client_type", clientType));
    }
}
