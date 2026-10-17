namespace Innovayse.Hostpanel.Application.Support.Queries.ListDownloads;

/// <summary>Query to retrieve downloads, optionally filtered by category.</summary>
/// <param name="CategoryId">Optional category id to filter by. <see langword="null"/> lists all downloads.</param>
public sealed record ListDownloadsQuery(int? CategoryId);
