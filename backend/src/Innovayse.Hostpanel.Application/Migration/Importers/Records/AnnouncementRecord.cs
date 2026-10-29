namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One announcement as the source plugin describes it.</summary>
/// <param name="Title">Announcement headline; also the duplicate check.</param>
/// <param name="Content">The body text, HTML as stored in the source.</param>
/// <param name="Published">Whether the source shows it publicly; an unpublished one is imported as a draft.</param>
/// <param name="CreatedAt">When the source created the record (UTC); kept so imported history keeps its dates.</param>
internal sealed record AnnouncementRecord(
    string Title, string Content, bool Published, DateTimeOffset? CreatedAt);
