using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innovayse.Hostpanel.Infrastructure.Migrations
{
    /// <summary>
    /// Fills the <c>Currency</c> column that <c>AddCurrencyToOrdersQuotesTransactions</c> added with an empty
    /// string on every existing order, quote and transaction. A row takes the currency of the document it was
    /// created from: the linked invoice first, then the client, then the base currency. Only rows whose currency
    /// is still empty are touched, and a row for which no currency can be found keeps its current value.
    /// </summary>
    public partial class BackfillOrderQuoteTransactionCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Transactions: the invoice they settle, else the client, else the base currency.
            migrationBuilder.Sql(
                """
                UPDATE "Transactions" AS t
                SET "Currency" = COALESCE(
                    (SELECT NULLIF(i."Currency", '') FROM "Invoices" AS i WHERE i."Id" = t."InvoiceId"),
                    (SELECT NULLIF(c."Currency", '') FROM "Clients" AS c WHERE c."Id" = t."ClientId"),
                    (SELECT b."Code" FROM "Currencies" AS b WHERE b."IsBase" LIMIT 1),
                    t."Currency")
                WHERE t."Currency" = '';
                """);

            // Orders: the invoice they were billed on, else the client, else the base currency.
            migrationBuilder.Sql(
                """
                UPDATE "Orders" AS o
                SET "Currency" = COALESCE(
                    (SELECT NULLIF(i."Currency", '') FROM "Invoices" AS i WHERE i."Id" = o."InvoiceId"),
                    (SELECT NULLIF(c."Currency", '') FROM "Clients" AS c WHERE c."Id" = o."ClientId"),
                    (SELECT b."Code" FROM "Currencies" AS b WHERE b."IsBase" LIMIT 1),
                    o."Currency")
                WHERE o."Currency" = '';
                """);

            // Quotes have no invoice: the client's currency, else the base currency.
            migrationBuilder.Sql(
                """
                UPDATE "Quotes" AS q
                SET "Currency" = COALESCE(
                    (SELECT NULLIF(c."Currency", '') FROM "Clients" AS c WHERE c."Id" = q."ClientId"),
                    (SELECT b."Code" FROM "Currencies" AS b WHERE b."IsBase" LIMIT 1),
                    q."Currency")
                WHERE q."Currency" = '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty: the previous migration's Down drops or relaxes the column itself, and blanking
            // the filled currencies here would destroy information, since a backfilled row cannot be told from a
            // row the application stamped afterwards.
        }
    }
}
