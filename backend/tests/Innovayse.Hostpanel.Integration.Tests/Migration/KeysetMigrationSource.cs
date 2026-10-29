using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;

namespace Innovayse.Hostpanel.Integration.Tests.Migration;

/// <summary>
/// A fake source that pages one dataset of numbered rows by keyset, the way the source plugin
/// does: each page holds the rows whose id is greater than the requested one, and names the id the
/// next page starts after until the rows run out. It reports a page count of one on purpose, so a
/// pager that followed the page count instead of the keyset would stop after the first page.
/// </summary>
/// <param name="rowIds">The ids of the rows the dataset holds, in id order.</param>
internal sealed class KeysetMigrationSource(IReadOnlyList<long> rowIds) : IMigrationSource
{
    /// <summary>The ids each fetch started after, in fetch order.</summary>
    public List<long> RequestedAfterIds { get; } = [];

    /// <inheritdoc/>
    public Task PingAsync(MigrationSourceEndpoint source, CancellationToken ct) =>
        Task.CompletedTask;

    /// <inheritdoc/>
    public Task<MigrationSourceTotals> GetTotalsAsync(MigrationSourceEndpoint source, CancellationToken ct) =>
        Task.FromResult(MigrationSourceTotals.Empty);

    /// <inheritdoc/>
    public Task<MigrationRecordPage<TRecord>> GetRecordPageAsync<TRecord>(
        MigrationSourceEndpoint source, MigrationPageSelection selection, CancellationToken ct)
    {
        RequestedAfterIds.Add(selection.AfterId);
        var ids = rowIds.Where(id => id > selection.AfterId).Take(selection.PerPage).ToList();
        var items = ids.Select(id => (TRecord)(object)id).ToList();

        // A full page may have more after it; a short page is the last one.
        long? nextAfterId = ids.Count == selection.PerPage ? ids[^1] : null;
        return Task.FromResult(new MigrationRecordPage<TRecord>(items, 1, nextAfterId));
    }
}
