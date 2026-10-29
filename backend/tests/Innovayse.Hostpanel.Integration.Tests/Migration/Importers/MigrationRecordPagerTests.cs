using Innovayse.Hostpanel.Application.Migration.Importers;
using Innovayse.Hostpanel.Domain.Migration;

namespace Innovayse.Hostpanel.Integration.Tests.Migration.Importers;

/// <summary>
/// <see cref="MigrationRecordPager"/> against a source that pages by keyset: every row is read
/// once, in id order, by following the id each page names as the next start.
/// </summary>
public sealed class MigrationRecordPagerTests
{
    /// <summary>A keyset source is paged by the id it names, past the page count it reports.</summary>
    [Fact]
    public async Task PagesAsync_KeysetSource_ReadsEveryRowOnceAsync()
    {
        // 120 rows with gaps in the ids, more than two pages of the pager's page size.
        var rowIds = Enumerable.Range(1, 120).Select(i => (long)(i * 3)).ToList();
        var source = new KeysetMigrationSource(rowIds);
        var pager = new MigrationRecordPager(source);
        var job = MigrationJob.Create(new MigrationJobSetup(
            Label: "keyset",
            SourceUrl: "http://fake-source.invalid",
            Selection: new MigrationExportSelection(
                Clients: true, Invoices: false, Services: false, Domains: false, Tickets: false,
                Products: false, Orders: false, Transactions: false, Quotes: false, Knowledgebase: false,
                Contacts: false, TicketReplies: false, Announcements: false, Downloads: false, NetworkIssues: false)));

        var read = new List<long>();
        await foreach (var page in pager.PagesAsync<long>(job, "clients", CancellationToken.None))
        {
            read.AddRange(page);
        }

        Assert.Equal(rowIds, read);
        Assert.Equal(0, source.RequestedAfterIds[0]);
        Assert.True(source.RequestedAfterIds.Count > 2);
    }
}
