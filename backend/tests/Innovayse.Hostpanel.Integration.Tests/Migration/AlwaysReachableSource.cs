using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;

namespace Innovayse.Hostpanel.Integration.Tests.Migration;

/// <summary>A fake source that always answers a ping, standing in for a reachable plugin.</summary>
internal sealed class AlwaysReachableSource : IMigrationSource
{
    /// <inheritdoc/>
    public Task PingAsync(MigrationSourceEndpoint source, CancellationToken ct) =>
        Task.CompletedTask;

    /// <inheritdoc/>
    public Task<MigrationSourceTotals> GetTotalsAsync(MigrationSourceEndpoint source, CancellationToken ct) =>
        Task.FromResult(MigrationSourceTotals.Empty);

    /// <inheritdoc/>
    public Task<MigrationRecordPage<TRecord>> GetRecordPageAsync<TRecord>(
        MigrationSourceEndpoint source, MigrationPageSelection selection, CancellationToken ct) =>
        Task.FromResult(new MigrationRecordPage<TRecord>([], 0));
}
