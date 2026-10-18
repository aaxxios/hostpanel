using System.Text.Json;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An <see cref="IMigrationSource"/> that hands back one page of JSON-seeded records per dataset --
/// a fake port standing in for the legacy install's own HTTP endpoint, which a test must not call.
/// </summary>
/// <remarks>
/// Every record shape <see cref="Application.Migration.Pull.MigrationPullWorker"/>'s importers
/// page through is a <c>private sealed record</c> nested inside its own importer class, so a test
/// in this assembly cannot name any of them. It does not need to: <c>GetRecordPageAsync&lt;TRecord&gt;</c>
/// deserializes each dataset's JSON into whatever <c>TRecord</c> the calling importer's own
/// generic instantiation supplies, exactly the way an HTTP client deserializing the real plugin's
/// response would.
/// </remarks>
public sealed class FakeMigrationSource : IMigrationSource
{
    /// <summary>One JSON array of records per dataset name, set by <see cref="SetDatasetJson"/>.</summary>
    private readonly Dictionary<string, string> _datasetsByName = new(StringComparer.Ordinal);

    /// <summary>The totals <see cref="GetTotalsAsync"/> answers with. Every count starts at zero.</summary>
    public MigrationSourceTotals Totals { get; set; } = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    /// <summary>How many times <see cref="PingAsync"/> was called.</summary>
    public int PingCallCount { get; private set; }

    /// <summary>
    /// Seeds one dataset's single page of records as a raw JSON array. A test cannot construct the
    /// importer's own record type directly -- every one is a <c>private sealed record</c> nested
    /// inside its importer -- so the caller writes the array literally, matching the record's
    /// positional parameter names exactly: <see cref="JsonSerializer"/> matches case-sensitively by
    /// default, and a positional record's generated properties are PascalCase.
    /// </summary>
    /// <param name="dataset">The dataset name an importer pages through, e.g. <c>"tickets"</c>.</param>
    /// <param name="json">The dataset's one and only page, as a JSON array of records.</param>
    public void SetDatasetJson(string dataset, string json)
    {
        _datasetsByName[dataset] = json;
    }

    /// <inheritdoc/>
    public Task PingAsync(string sourceUrl, string accessKey, CancellationToken ct = default)
    {
        PingCallCount++;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<MigrationSourceTotals> GetTotalsAsync(string sourceUrl, string accessKey, CancellationToken ct = default) =>
        Task.FromResult(Totals);

    /// <inheritdoc/>
    public Task<MigrationRecordPage<TRecord>> GetRecordPageAsync<TRecord>(
        string sourceUrl, string accessKey, string dataSet, int page, int perPage, CancellationToken ct = default)
    {
        // Every seeded dataset here is exactly one page; a second page always ends the pull.
        if (page > 1 || !_datasetsByName.TryGetValue(dataSet, out var json))
        {
            return Task.FromResult(new MigrationRecordPage<TRecord>([], 0));
        }

        var items = JsonSerializer.Deserialize<List<TRecord>>(json) ?? [];
        return Task.FromResult(new MigrationRecordPage<TRecord>(items, 1));
    }
}
