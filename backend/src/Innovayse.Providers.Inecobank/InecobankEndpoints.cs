namespace Innovayse.Providers.Inecobank;

/// <summary>
/// Names of the Inecobank (Armenian Card) merchant REST API endpoints used by
/// <see cref="InecobankApiClient"/>, relative to <c>/payment/rest/</c>.
/// </summary>
internal static class InecobankEndpoints
{
    /// <summary>
    /// The path every endpoint lives under, appended to the configured origin. The one place it is
    /// written, so the client that builds the request and the configuration check that refuses an
    /// origin already carrying it cannot disagree.
    /// </summary>
    public const string BasePath = "/payment/rest";

    /// <summary>Registers a new order (one-stage payment) and opens a hosted-page session.</summary>
    public const string Register = "register.do";

    /// <summary>Fetches the extended status of a previously registered order.</summary>
    public const string GetOrderStatusExtended = "getOrderStatusExtended.do";

    /// <summary>Refunds a deposited order, fully or partially.</summary>
    public const string Refund = "refund.do";
}
