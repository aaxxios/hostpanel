namespace Innovayse.Hostpanel.Application.Migration.Importers.SourceValues;

/// <summary>The network-issue values, compared lower-case the source system sends for a source network-issue record. They are the source's own spellings, so they cannot be renamed.</summary>
public static class SourceNetworkIssueValues
{
    /// <summary>The source value <c>resolved</c>.</summary>
    public const string Resolved = "resolved";

    /// <summary>The source value <c>server</c>.</summary>
    public const string ServerType = "server";

    /// <summary>The source value <c>critical</c>.</summary>
    public const string CriticalPriority = "critical";

    /// <summary>The source value <c>high</c>.</summary>
    public const string HighPriority = "high";

    /// <summary>The source value <c>low</c>.</summary>
    public const string LowPriority = "low";
}
