namespace Innovayse.Hostpanel.Application.Migration.Common;

/// <summary>
/// One page of records of a single kind, as the legacy source hands them over.
/// </summary>
/// <typeparam name="TRecord">Shape the caller expects each record on this page to have.</typeparam>
/// <param name="Items">
/// The records on this page. Null or empty where the source had nothing to give for this page:
/// the two mean the same thing here, and neither ends the paging on its own.
/// </param>
/// <param name="TotalPages">
/// How many pages the source holds in total for this kind of record. Zero means the source
/// reported no pages at all, which ends the paging immediately.
/// </param>
/// <param name="NextAfterId">
/// The source row id the next page starts after, when the source pages by keyset; null when the
/// source reached the end of the dataset, or does not page by keyset and is paged by
/// <paramref name="TotalPages"/> instead.
/// </param>
public sealed record MigrationRecordPage<TRecord>(
    IReadOnlyList<TRecord>? Items,
    int TotalPages,
    long? NextAfterId = null);
