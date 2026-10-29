using System.Runtime.CompilerServices;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Domain.Migration;

namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Fetches pages of one record shape from the migration source plugin, for any
/// <see cref="IMigrationEntityImporter"/>. It exists so every importer pages through its own dataset the same way, without each one repeating the
/// page-and-yield loop.
/// </summary>
public sealed class MigrationRecordPager
{
    /// <summary>Page size used against the source plugin's pull endpoints.</summary>
    private const int PerPage = 50;

    /// <summary>The migration source plugin every importer pulls from.</summary>
    private readonly IMigrationSource _source;

    /// <summary>Initializes a new instance of the <see cref="MigrationRecordPager"/> class.</summary>
    /// <param name="source">The migration source plugin every importer pulls from.</param>
    public MigrationRecordPager(IMigrationSource source)
    {
        _source = source;
    }

    /// <summary>
    /// Async iterator: fetches pages of <typeparamref name="T"/> from the plugin and yields each
    /// page's item list, stopping once the plugin reports no more pages.
    /// </summary>
    /// <remarks>
    /// A plugin that pages by keyset answers with the id the next page starts after, and is paged
    /// by it: rows added or removed in the source during the pull then never shift a page and
    /// skip or repeat a record. A plugin that does not is paged by page number up to the page
    /// count it reports.
    /// </remarks>
    /// <typeparam name="T">The record shape this dataset's pages are read into.</typeparam>
    /// <param name="job">The migration job whose source URL and key address the plugin.</param>
    /// <param name="action">Which dataset to fetch, in the source plugin's own vocabulary.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Each non-empty page's items, in page order.</returns>
    public async IAsyncEnumerable<IReadOnlyList<T>> PagesAsync<T>(
        MigrationJob job,
        string action,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var endpoint = new MigrationSourceEndpoint(job.SourceUrl, job.Key);
        var page = 1;
        long afterId = 0;

        while (true)
        {
            var resp = await _source.GetRecordPageAsync<T>(
                endpoint, new MigrationPageSelection(action, page, PerPage, afterId), ct);

            if (resp.Items is { Count: > 0 })
            {
                yield return resp.Items;
            }

            if (resp.NextAfterId is { } nextAfterId)
            {
                // Keyset paging: an id that does not move forward would ask for the same page again.
                if (nextAfterId <= afterId)
                {
                    break;
                }

                afterId = nextAfterId;
                page++;
                continue;
            }

            if (afterId > 0 || page >= resp.TotalPages || resp.TotalPages == 0)
            {
                // A keyset page without a next id is the last one.
                break;
            }

            page++;
        }
    }
}
