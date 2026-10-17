namespace Innovayse.Hostpanel.Application.Migration.Importers.SourceValues;

/// <summary>The domain statuses, compared lower-case the source system sends for a source domain record. They are the source's own spellings, so they cannot be renamed.</summary>
public static class SourceDomainStatuses
{
    /// <summary>The source value <c>active</c>.</summary>
    public const string Active = "active";

    /// <summary>The source value <c>expired</c>.</summary>
    public const string Expired = "expired";

    /// <summary>The source value <c>redemption</c>.</summary>
    public const string Redemption = "redemption";

    /// <summary>The source value <c>transferred</c>.</summary>
    public const string Transferred = "transferred";

    /// <summary>The source value <c>cancelled</c>.</summary>
    public const string Cancelled = "cancelled";

    /// <summary>The source value <c>fraud</c>.</summary>
    public const string Fraud = "fraud";
}
