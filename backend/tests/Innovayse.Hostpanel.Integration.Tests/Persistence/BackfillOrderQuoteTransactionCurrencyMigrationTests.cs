using FluentAssertions;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Persistence;

/// <summary>
/// The <c>BackfillOrderQuoteTransactionCurrency</c> migration fills the <c>Currency</c> column the earlier
/// migration added with an empty string on every existing order, quote and transaction. These tests build a
/// database at the schema before the backfill, seed rows with an empty currency (and rows that already carry
/// one), apply the migration, and prove each row got the right currency and no stamped row was touched.
/// </summary>
public sealed class BackfillOrderQuoteTransactionCurrencyMigrationTests
{
    /// <summary>The migration that added the columns, the schema every production database has before the backfill.</summary>
    private const string PreviousMigration = "20261025175216_AddCurrencyToOrdersQuotesTransactions";

    /// <summary>The backfill migration under test.</summary>
    private const string BackfillMigration = "20261025185902_BackfillOrderQuoteTransactionCurrency";

    /// <summary>The base currency the <c>AddCurrencies</c> migration seeds.</summary>
    private const string BaseCurrency = "USD";

    /// <summary>The currency of the client that has one.</summary>
    private const string ClientCurrency = "EUR";

    /// <summary>The currency of the invoice, different from the client's so the precedence is visible.</summary>
    private const string InvoiceCurrency = "GBP";

    /// <summary>A currency stamped before the backfill ran, which must survive it.</summary>
    private const string StampedCurrency = "AMD";

    /// <summary>
    /// Transactions take the linked invoice's currency, else the client's, else the base currency; a transaction
    /// that already has a currency keeps it.
    /// </summary>
    [Fact]
    public async Task Up_OnTransactionsWithEmptyCurrency_TakesInvoiceThenClientThenBaseAsync()
    {
        await using var database = await SeededDatabase.CreateAsync();

        await database.MigrateToBackfillAsync();

        (await database.CurrencyOfAsync("Transactions", database.TransactionOnInvoice)).Should().Be(InvoiceCurrency, "the invoice wins over the client");
        (await database.CurrencyOfAsync("Transactions", database.TransactionOfCurrencyClient)).Should().Be(ClientCurrency, "no invoice, so the client's currency");
        (await database.CurrencyOfAsync("Transactions", database.TransactionOfBareClient)).Should().Be(BaseCurrency, "no invoice and no client currency, so the base");
        (await database.CurrencyOfAsync("Transactions", database.TransactionStamped)).Should().Be(StampedCurrency, "a stamped row is not rewritten");
    }

    /// <summary>
    /// Orders take the linked invoice's currency, else the client's, else the base currency; an order that already
    /// has a currency keeps it.
    /// </summary>
    [Fact]
    public async Task Up_OnOrdersWithEmptyCurrency_TakesInvoiceThenClientThenBaseAsync()
    {
        await using var database = await SeededDatabase.CreateAsync();

        await database.MigrateToBackfillAsync();

        (await database.CurrencyOfAsync("Orders", database.OrderOnInvoice)).Should().Be(InvoiceCurrency, "the invoice wins over the client");
        (await database.CurrencyOfAsync("Orders", database.OrderOfCurrencyClient)).Should().Be(ClientCurrency, "no invoice, so the client's currency");
        (await database.CurrencyOfAsync("Orders", database.OrderOfBareClient)).Should().Be(BaseCurrency, "no invoice and no client currency, so the base");
        (await database.CurrencyOfAsync("Orders", database.OrderStamped)).Should().Be(StampedCurrency, "a stamped row is not rewritten");
    }

    /// <summary>Quotes take the client's currency, else the base currency; a quote that already has a currency keeps it.</summary>
    [Fact]
    public async Task Up_OnQuotesWithEmptyCurrency_TakesClientThenBaseAsync()
    {
        await using var database = await SeededDatabase.CreateAsync();

        await database.MigrateToBackfillAsync();

        (await database.CurrencyOfAsync("Quotes", database.QuoteOfCurrencyClient)).Should().Be(ClientCurrency, "the client's currency");
        (await database.CurrencyOfAsync("Quotes", database.QuoteOfBareClient)).Should().Be(BaseCurrency, "no client currency, so the base");
        (await database.CurrencyOfAsync("Quotes", database.QuoteStamped)).Should().Be(StampedCurrency, "a stamped row is not rewritten");
    }

    /// <summary>The backfill cannot be undone, so going down keeps the data and the filled currencies.</summary>
    [Fact]
    public async Task Down_AfterTheBackfill_KeepsTheFilledCurrenciesAsync()
    {
        await using var database = await SeededDatabase.CreateAsync();
        await database.MigrateToBackfillAsync();

        await database.MigrateDownAsync();

        (await database.CurrencyOfAsync("Orders", database.OrderOnInvoice)).Should().Be(InvoiceCurrency);
        (await database.CurrencyOfAsync("Quotes", database.QuoteOfBareClient)).Should().Be(BaseCurrency);
        (await database.CurrencyOfAsync("Transactions", database.TransactionOfCurrencyClient)).Should().Be(ClientCurrency);
    }

    /// <summary>
    /// A throwaway database migrated to the schema before the backfill and seeded with rows whose currency is empty,
    /// dropped when disposed.
    /// </summary>
    private sealed class SeededDatabase : IAsyncDisposable
    {
        /// <summary>The unique name of the database, for the drop.</summary>
        private readonly string _name;

        /// <summary>The connection string of the database.</summary>
        private readonly string _connectionString;

        /// <summary>Initializes a new instance of the <see cref="SeededDatabase"/> class.</summary>
        /// <param name="name">The unique name of the database.</param>
        /// <param name="connectionString">The connection string of the database.</param>
        private SeededDatabase(string name, string connectionString)
        {
            _name = name;
            _connectionString = connectionString;
        }

        /// <summary>Gets the id of the transaction on an invoice whose client also has a currency.</summary>
        public int TransactionOnInvoice { get; private set; }

        /// <summary>Gets the id of the transaction without an invoice, of the client that has a currency.</summary>
        public int TransactionOfCurrencyClient { get; private set; }

        /// <summary>Gets the id of the transaction without an invoice, of the client that has no currency.</summary>
        public int TransactionOfBareClient { get; private set; }

        /// <summary>Gets the id of the transaction that already carries a currency.</summary>
        public int TransactionStamped { get; private set; }

        /// <summary>Gets the id of the order on an invoice whose client also has a currency.</summary>
        public int OrderOnInvoice { get; private set; }

        /// <summary>Gets the id of the order without an invoice, of the client that has a currency.</summary>
        public int OrderOfCurrencyClient { get; private set; }

        /// <summary>Gets the id of the order without an invoice, of the client that has no currency.</summary>
        public int OrderOfBareClient { get; private set; }

        /// <summary>Gets the id of the order that already carries a currency.</summary>
        public int OrderStamped { get; private set; }

        /// <summary>Gets the id of the quote of the client that has a currency.</summary>
        public int QuoteOfCurrencyClient { get; private set; }

        /// <summary>Gets the id of the quote of the client that has no currency.</summary>
        public int QuoteOfBareClient { get; private set; }

        /// <summary>Gets the id of the quote that already carries a currency.</summary>
        public int QuoteStamped { get; private set; }

        /// <summary>Creates the database, migrates it to the schema before the backfill and seeds it.</summary>
        /// <returns>The seeded database.</returns>
        public static async Task<SeededDatabase> CreateAsync()
        {
            var name = $"it_{Guid.NewGuid():N}";
            var connectionString = await SharedPostgresServer.CreateDatabaseAsync(name);
            var database = new SeededDatabase(name, connectionString);
            try
            {
                await using var context = database.NewContext();
                await context.GetService<IMigrator>().MigrateAsync(PreviousMigration);
                await database.SeedAsync(context);
            }
            catch
            {
                await database.DisposeAsync();
                throw;
            }

            return database;
        }

        /// <summary>Applies the backfill migration.</summary>
        /// <returns>A task that completes when the migration has run.</returns>
        public async Task MigrateToBackfillAsync()
        {
            await using var context = NewContext();
            await context.GetService<IMigrator>().MigrateAsync(BackfillMigration);
        }

        /// <summary>Reverts the backfill migration.</summary>
        /// <returns>A task that completes when the migration has been reverted.</returns>
        public async Task MigrateDownAsync()
        {
            await using var context = NewContext();
            await context.GetService<IMigrator>().MigrateAsync(PreviousMigration);
        }

        /// <summary>Reads the currency of one row.</summary>
        /// <param name="table">The table, one of the three the backfill fills.</param>
        /// <param name="id">The id of the row.</param>
        /// <returns>The row's currency.</returns>
        public async Task<string> CurrencyOfAsync(string table, int id)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT \"Currency\" FROM \"{table}\" WHERE \"Id\" = @id";
            command.Parameters.AddWithValue("id", id);
            return (string)(await command.ExecuteScalarAsync())!;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            using var probe = new NpgsqlConnection(_connectionString);
            NpgsqlConnection.ClearPool(probe);
            await SharedPostgresServer.DropDatabaseAsync(_name);
        }

        /// <summary>Opens a context over the database.</summary>
        /// <returns>A new context.</returns>
        private AppDbContext NewContext() =>
            new(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(_connectionString).Options);

        /// <summary>
        /// Seeds two clients (one with a currency, one without), an invoice in a third currency, and an order, a
        /// quote and a transaction per case, then blanks the currency of every row but the stamped ones, the state
        /// the column migration leaves existing rows in.
        /// </summary>
        /// <param name="context">A context over the database at the schema before the backfill.</param>
        /// <returns>A task that completes when the rows are saved.</returns>
        private async Task SeedAsync(AppDbContext context)
        {
            var withCurrency = Client.Create(Guid.NewGuid().ToString(), "Ada", "Currency", "ada@backfill.example");
            var bare = Client.Create(Guid.NewGuid().ToString(), "Bob", "Bare", "bob@backfill.example");
            context.Clients.AddRange(withCurrency, bare);
            await context.SaveChangesAsync();
            await ExecuteAsync($"UPDATE \"Clients\" SET \"Currency\" = '{ClientCurrency}' WHERE \"Id\" = {withCurrency.Id}");
            await ExecuteAsync($"UPDATE \"Clients\" SET \"Currency\" = NULL WHERE \"Id\" = {bare.Id}");

            var invoice = Invoice.Create(withCurrency.Id, DateTimeOffset.UtcNow.AddDays(7), InvoiceCurrency);
            context.Invoices.Add(invoice);
            await context.SaveChangesAsync();

            var now = DateTimeOffset.UtcNow;
            var transactions = new[]
            {
                NewTransaction(withCurrency.Id, "onInvoice", invoice.Id, BaseCurrency),
                NewTransaction(withCurrency.Id, "ofCurrencyClient", null, BaseCurrency),
                NewTransaction(bare.Id, "ofBareClient", null, BaseCurrency),
                NewTransaction(withCurrency.Id, "stamped", null, StampedCurrency),
            };
            var orders = new[]
            {
                Order.Create("BF-1", withCurrency.Id, "Manual", null, BaseCurrency),
                Order.Create("BF-2", withCurrency.Id, "Manual", null, BaseCurrency),
                Order.Create("BF-3", bare.Id, "Manual", null, BaseCurrency),
                Order.Create("BF-4", withCurrency.Id, "Manual", null, StampedCurrency),
            };
            var quotes = new[]
            {
                Quote.Create(withCurrency.Id, "Of the currency client", now.AddDays(7), BaseCurrency),
                Quote.Create(bare.Id, "Of the bare client", now.AddDays(7), BaseCurrency),
                Quote.Create(withCurrency.Id, "Stamped", now.AddDays(7), StampedCurrency),
            };
            context.Transactions.AddRange(transactions);
            context.Orders.AddRange(orders);
            context.Quotes.AddRange(quotes);
            await context.SaveChangesAsync();

            TransactionOnInvoice = transactions[0].Id;
            TransactionOfCurrencyClient = transactions[1].Id;
            TransactionOfBareClient = transactions[2].Id;
            TransactionStamped = transactions[3].Id;
            OrderOnInvoice = orders[0].Id;
            OrderOfCurrencyClient = orders[1].Id;
            OrderOfBareClient = orders[2].Id;
            OrderStamped = orders[3].Id;
            QuoteOfCurrencyClient = quotes[0].Id;
            QuoteOfBareClient = quotes[1].Id;
            QuoteStamped = quotes[2].Id;

            // The order links to the invoice through the payment flow; here the link is written directly.
            await ExecuteAsync($"UPDATE \"Orders\" SET \"InvoiceId\" = {invoice.Id} WHERE \"Id\" = {OrderOnInvoice}");

            // Rows as the column migration left them: every row but the stamped ones has an empty currency.
            var stamped = $"'{StampedCurrency}'";
            await ExecuteAsync($"UPDATE \"Transactions\" SET \"Currency\" = '' WHERE \"Currency\" <> {stamped}");
            await ExecuteAsync($"UPDATE \"Orders\" SET \"Currency\" = '' WHERE \"Currency\" <> {stamped}");
            await ExecuteAsync($"UPDATE \"Quotes\" SET \"Currency\" = '' WHERE \"Currency\" <> {stamped}");
        }

        /// <summary>Builds a transaction row for the seed.</summary>
        /// <param name="clientId">The client the transaction belongs to.</param>
        /// <param name="key">A unique suffix for the gateway transaction id.</param>
        /// <param name="invoiceId">The invoice it settles, if any.</param>
        /// <param name="currency">The currency to create it with.</param>
        /// <returns>The new transaction.</returns>
        private static Transaction NewTransaction(int clientId, string key, int? invoiceId, string currency) =>
            Transaction.Create(clientId, DateTimeOffset.UtcNow, key, $"backfill-{key}", invoiceId, "Manual", 10m, 0m, 0m, false, currency);

        /// <summary>Runs one statement against the database.</summary>
        /// <param name="sql">The statement.</param>
        /// <returns>A task that completes when the statement has run.</returns>
        private async Task ExecuteAsync(string sql)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }
    }
}
