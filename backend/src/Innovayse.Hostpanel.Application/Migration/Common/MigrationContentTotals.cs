namespace Innovayse.Hostpanel.Application.Migration.Common;

/// <summary>How many published content records the legacy source holds.</summary>
/// <param name="Knowledgebase">Knowledgebase articles available to pull.</param>
/// <param name="Announcements">Announcements available to pull.</param>
/// <param name="Downloads">Downloads available to pull.</param>
/// <param name="DownloadCategories">
/// Download categories available to pull. Reported by the source and carried here for
/// completeness; downloads are counted against <paramref name="Downloads"/> alone.
/// </param>
public sealed record MigrationContentTotals(int Knowledgebase = 0, int Announcements = 0, int Downloads = 0, int DownloadCategories = 0);
