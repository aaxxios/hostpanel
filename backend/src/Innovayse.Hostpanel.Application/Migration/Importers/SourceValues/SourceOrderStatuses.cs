namespace Innovayse.Hostpanel.Application.Migration.Importers.SourceValues;

/// <summary>The order statuses the source system sends for a source order record. They are the source's own spellings, so they cannot be renamed.</summary>
public static class SourceOrderStatuses
{
    /// <summary>The source value <c>Active</c>.</summary>
    public const string Active = "Active";

    /// <summary>The source value <c>Cancelled</c>.</summary>
    public const string Cancelled = "Cancelled";
}
