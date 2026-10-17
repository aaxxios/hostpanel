namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One announcement as the source plugin describes it.</summary>
/// <param name="Title">The title.</param>
/// <param name="Content">The content.</param>
/// <param name="Published">Whether published.</param>
/// <param name="CreatedAt">The created at.</param>
internal sealed record AnnouncementRecord(
    string Title, string Content, bool Published, DateTimeOffset? CreatedAt);
