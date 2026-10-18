namespace Innovayse.Hostpanel.Application.Migration.Importers.SourceValues;

/// <summary>The service statuses the source system sends for a source service record. They are the source's own spellings, so they cannot be renamed.</summary>
public static class SourceServiceStatuses
{
    /// <summary>The source value <c>Active</c>.</summary>
    public const string Active = "Active";

    /// <summary>The source value <c>Suspended</c>.</summary>
    public const string Suspended = "Suspended";

    /// <summary>The source value <c>Suspended (Admin)</c>.</summary>
    public const string SuspendedByAdmin = "Suspended (Admin)";

    /// <summary>The source value <c>Terminated</c>.</summary>
    public const string Terminated = "Terminated";

    /// <summary>The source value <c>Cancelled</c>.</summary>
    public const string Cancelled = "Cancelled";
}
