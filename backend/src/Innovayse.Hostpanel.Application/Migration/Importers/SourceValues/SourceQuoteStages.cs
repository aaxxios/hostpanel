namespace Innovayse.Hostpanel.Application.Migration.Importers.SourceValues;

/// <summary>The quote stages the source system sends for a source quote record. They are the source's own spellings, so they cannot be renamed.</summary>
public static class SourceQuoteStages
{
    /// <summary>The source value <c>Delivered</c>.</summary>
    public const string Delivered = "Delivered";

    /// <summary>The source value <c>Accepted</c>.</summary>
    public const string Accepted = "Accepted";

    /// <summary>The source value <c>Lost</c>.</summary>
    public const string Lost = "Lost";

    /// <summary>The source value <c>Dead</c>.</summary>
    public const string Dead = "Dead";
}
