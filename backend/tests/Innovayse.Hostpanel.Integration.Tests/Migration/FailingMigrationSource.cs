using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;

namespace Innovayse.Hostpanel.Integration.Tests.Migration;

/// <summary>A fake source whose every call throws the exception it was built with, standing in for a source that fails.</summary>
/// <param name="failure">The exception every call throws.</param>
internal sealed class FailingMigrationSource(Exception failure) : IMigrationSource
{
    /// <inheritdoc/>
    public Task PingAsync(MigrationSourceEndpoint source, CancellationToken ct) =>
        Task.FromException(failure);

    /// <inheritdoc/>
    public Task<MigrationSourceTotals> GetTotalsAsync(MigrationSourceEndpoint source, CancellationToken ct) =>
        Task.FromException<MigrationSourceTotals>(failure);

    /// <inheritdoc/>
    public Task<MigrationRecordPage<TRecord>> GetRecordPageAsync<TRecord>(
        MigrationSourceEndpoint source, MigrationPageSelection selection, CancellationToken ct) =>
        Task.FromException<MigrationRecordPage<TRecord>>(failure);
}
