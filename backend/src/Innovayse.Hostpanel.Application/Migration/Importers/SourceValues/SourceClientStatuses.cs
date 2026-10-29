namespace Innovayse.Hostpanel.Application.Migration.Importers.SourceValues;

/// <summary>The client statuses the source system sends for a source client record. They are the source's own spellings, so they cannot be renamed.</summary>
public static class SourceClientStatuses
{
    /// <summary>The source value <c>Active</c>.</summary>
    public const string Active = "Active";

    /// <summary>The source value <c>Inactive</c>.</summary>
    public const string Inactive = "Inactive";

    /// <summary>The source value <c>Closed</c>.</summary>
    public const string Closed = "Closed";
}
