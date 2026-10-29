namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One knowledge-base article as the source plugin describes it.</summary>
/// <param name="Title">Article title; also the duplicate check, case-insensitive.</param>
/// <param name="Content">The body text, HTML as stored in the source.</param>
/// <param name="CategoryId">The source id of the category the record belongs to; null when it has none.</param>
/// <param name="Published">Whether the source shows it publicly; an unpublished one is imported as a draft.</param>
internal sealed record KbArticleRecord(string Title, string Content, int? CategoryId, bool Published);
