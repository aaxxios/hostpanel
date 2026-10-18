namespace Innovayse.Hostpanel.Application.Migration.Importers.SourceValues;

/// <summary>The ticket statuses the source system sends for a source ticket record. They are the source's own spellings, so they cannot be renamed.</summary>
public static class SourceTicketStatuses
{
    /// <summary>The source value <c>Closed</c>.</summary>
    public const string Closed = "Closed";

    /// <summary>The source value <c>Answered</c>.</summary>
    public const string Answered = "Answered";
}
