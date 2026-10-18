namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One knowledge-base article as the source plugin describes it.</summary>
/// <param name="Title">The title.</param>
/// <param name="Content">The content.</param>
/// <param name="CategoryId">The category identifier.</param>
/// <param name="Published">Whether published.</param>
internal sealed record KbArticleRecord(string Title, string Content, int? CategoryId, bool Published);
