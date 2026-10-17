using System.Runtime.CompilerServices;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Domain.Migration;

namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Fetches pages of one record shape from the migration source plugin, for any
/// <see cref="IMigrationEntityImporter"/>. Extracted from <c>MigrationPullWorker</c> (A6 Task 6)
/// so every importer pages through its own dataset the same way, without each one repeating the
/// page-and-yield loop.
/// </summary>
/// <param name="source">The migration source plugin every importer pulls from.</param>
public sealed class MigrationRecordPager(IMigrationSource source)
{
    /// <summary>Page size used against the source plugin's pull endpoints.</summary>
    private const int PerPage = 50;

    /// <summary>
    /// Async iterator: fetches pages of <typeparamref name="T"/> from the plugin and yields each
    /// page's item list, stopping once the plugin reports no more pages.
    /// </summary>
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
        int page = 1;

        while (true)
        {
            var resp = await source.GetRecordPageAsync<T>(job.SourceUrl, job.Key, action, page, PerPage, ct);

            if (resp.Items is { Count: > 0 })
            {
                yield return resp.Items;
            }

            if (page >= resp.TotalPages || resp.TotalPages == 0)
            {
                break;
            }

            page++;
        }
    }
}
