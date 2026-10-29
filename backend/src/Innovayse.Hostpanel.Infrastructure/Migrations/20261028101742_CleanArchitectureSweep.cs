using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Innovayse.Hostpanel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanArchitectureSweep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A free product is sold monthly at zero, so a legacy 'Free Account' cycle on a row that
            // recurs at zero is the monthly cycle under another name. A priced 'Free Account' row is
            // left alone and stops the migration at the guard below.
            migrationBuilder.Sql(
                """
                UPDATE "ClientServices"
                SET "BillingCycle" = 'Monthly'
                WHERE lower(btrim("BillingCycle")) = 'free account' AND "RecurringAmount" = 0;

                UPDATE "OrderItems"
                SET "BillingCycle" = 'Monthly'
                WHERE lower(btrim("BillingCycle")) = 'free account' AND "RecurringAmount" = 0;
                """);

            // Data the new constraints and narrower columns cannot hold stops the migration before any
            // change, so an operator resolves those rows by hand instead of losing them. Every check
            // only reads the schema as it stands before this migration. The billing cycle check names
            // every unknown value it found, so the operator knows which rows to resolve.
            migrationBuilder.Sql(
                """
                DO $$
                DECLARE
                    UnknownCycles text;
                BEGIN
                    IF EXISTS (SELECT 1 FROM "Domains" GROUP BY lower("Name") HAVING count(*) > 1) THEN
                        RAISE EXCEPTION 'Domains contains names that differ only by case; resolve them before migrating.';
                    END IF;
                    SELECT string_agg(DISTINCT quote_literal(c."BillingCycle"), ', ' ORDER BY quote_literal(c."BillingCycle"))
                    INTO UnknownCycles
                    FROM (
                        SELECT "BillingCycle" FROM "ClientServices"
                        UNION
                        SELECT "BillingCycle" FROM "OrderItems"
                    ) c
                    WHERE lower(btrim(c."BillingCycle")) NOT IN ('monthly', 'annual', 'annually');
                    IF UnknownCycles IS NOT NULL THEN
                        RAISE EXCEPTION 'ClientServices or OrderItems hold billing cycles other than monthly or annual (%); resolve those rows before migrating.', UnknownCycles;
                    END IF;
                    IF EXISTS (SELECT 1 FROM "BillableItems" r WHERE NOT EXISTS (SELECT 1 FROM "Clients" p WHERE p."Id" = r."ClientId"))
                       OR EXISTS (SELECT 1 FROM "ClientServices" r WHERE NOT EXISTS (SELECT 1 FROM "Clients" p WHERE p."Id" = r."ClientId"))
                       OR EXISTS (SELECT 1 FROM "Domains" r WHERE NOT EXISTS (SELECT 1 FROM "Clients" p WHERE p."Id" = r."ClientId"))
                       OR EXISTS (SELECT 1 FROM "Invoices" r WHERE NOT EXISTS (SELECT 1 FROM "Clients" p WHERE p."Id" = r."ClientId"))
                       OR EXISTS (SELECT 1 FROM "Orders" r WHERE NOT EXISTS (SELECT 1 FROM "Clients" p WHERE p."Id" = r."ClientId"))
                       OR EXISTS (SELECT 1 FROM "Quotes" r WHERE NOT EXISTS (SELECT 1 FROM "Clients" p WHERE p."Id" = r."ClientId"))
                       OR EXISTS (SELECT 1 FROM "Tickets" r WHERE NOT EXISTS (SELECT 1 FROM "Clients" p WHERE p."Id" = r."ClientId"))
                       OR EXISTS (SELECT 1 FROM "Transactions" r WHERE NOT EXISTS (SELECT 1 FROM "Clients" p WHERE p."Id" = r."ClientId")) THEN
                        RAISE EXCEPTION 'Rows reference a client that does not exist; resolve them before migrating.';
                    END IF;
                    IF EXISTS (SELECT 1 FROM "ClientServices" r WHERE NOT EXISTS (SELECT 1 FROM "Products" p WHERE p."Id" = r."ProductId"))
                       OR EXISTS (SELECT 1 FROM "OrderItems" r WHERE NOT EXISTS (SELECT 1 FROM "Products" p WHERE p."Id" = r."ProductId")) THEN
                        RAISE EXCEPTION 'Services or order items reference a product that does not exist; resolve them before migrating.';
                    END IF;
                    IF EXISTS (SELECT 1 FROM "PredefinedReplies" r WHERE NOT EXISTS (SELECT 1 FROM "PredefinedReplyCategories" p WHERE p."Id" = r."CategoryId"))
                       OR EXISTS (SELECT 1 FROM "DownloadCategories" r WHERE r."ParentCategoryId" IS NOT NULL AND NOT EXISTS (SELECT 1 FROM "DownloadCategories" p WHERE p."Id" = r."ParentCategoryId"))
                       OR EXISTS (SELECT 1 FROM "KbCategories" r WHERE r."ParentCategoryId" IS NOT NULL AND NOT EXISTS (SELECT 1 FROM "KbCategories" p WHERE p."Id" = r."ParentCategoryId"))
                       OR EXISTS (SELECT 1 FROM "PredefinedReplyCategories" r WHERE r."ParentCategoryId" IS NOT NULL AND NOT EXISTS (SELECT 1 FROM "PredefinedReplyCategories" p WHERE p."Id" = r."ParentCategoryId")) THEN
                        RAISE EXCEPTION 'Rows reference a category that does not exist; resolve them before migrating.';
                    END IF;
                    IF EXISTS (SELECT 1 FROM "Invoices" WHERE length("ExternalId") > 100)
                       OR EXISTS (SELECT 1 FROM "Invoices" WHERE "ExternalId" IS NOT NULL GROUP BY "ClientId", "ExternalId" HAVING count(*) > 1) THEN
                        RAISE EXCEPTION 'Invoices hold an external id longer than 100 characters or the same external id twice for one client; resolve them before migrating.';
                    END IF;
                    IF EXISTS (SELECT 1 FROM "SlideTranslations" WHERE length("Features") > 8192) THEN
                        RAISE EXCEPTION 'SlideTranslations hold bullet points longer than 8192 characters; resolve them before migrating.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Downloads_DownloadCategories_CategoryId",
                table: "Downloads");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ClientId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ClientId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_Status",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_MigrationLogs_JobId",
                table: "MigrationLogs");

            migrationBuilder.DropIndex(
                name: "IX_EmailTemplates_Slug",
                table: "EmailTemplates");

            migrationBuilder.DropIndex(
                name: "IX_DomainReminders_DomainId",
                table: "DomainReminders");

            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_ClientId",
                table: "ActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_CreatedAt",
                table: "ActivityLogs");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Tickets",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Testimonials",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<string>(
                name: "Features",
                table: "SlideTranslations",
                type: "character varying(8192)",
                maxLength: 8192,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Slides",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Settings",
                type: "character varying(10719)",
                maxLength: 10719,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "AgentPrivateKey",
                table: "ServerSshSettings",
                type: "character varying(42719)",
                maxLength: 42719,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(8000)",
                oldMaxLength: 8000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Servers",
                type: "character varying(2719)",
                maxLength: 2719,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApiToken",
                table: "Servers",
                type: "character varying(5383)",
                maxLength: 5383,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccessHash",
                table: "Servers",
                type: "character varying(10719)",
                maxLength: 10719,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "ServerLocations",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Quotes",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountPercent",
                table: "QuoteItems",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Projects",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<string>(
                name: "Website",
                table: "Products",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Products",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Products",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Partners",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Orders",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "OrderItems",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "RecurringAmount",
                table: "OrderItems",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "FirstPaymentAmount",
                table: "OrderItems",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<string>(
                name: "SkipReason",
                table: "MigrationLogs",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnnouncementsFailed",
                table: "MigrationJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ClientsFailed",
                table: "MigrationJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContactsFailed",
                table: "MigrationJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DomainsFailed",
                table: "MigrationJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DownloadsFailed",
                table: "MigrationJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InvoicesFailed",
                table: "MigrationJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "KnowledgebaseFailed",
                table: "MigrationJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastHeartbeatAt",
                table: "MigrationJobs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NetworkIssuesFailed",
                table: "MigrationJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrdersFailed",
                table: "MigrationJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductsFailed",
                table: "MigrationJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuotesFailed",
                table: "MigrationJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ServicesFailed",
                table: "MigrationJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TicketRepliesFailed",
                table: "MigrationJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TicketsFailed",
                table: "MigrationJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransactionsFailed",
                table: "MigrationJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "MigrationJobs",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "Invoices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GatewayCheckCount",
                table: "Invoices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "GatewayCheckedAt",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "GatewayConcludedAt",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "GatewayStalledNotifiedAt",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RefundStartedAt",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Faqs",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "EmailTemplates",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "en");

            migrationBuilder.AlterColumn<int>(
                name: "RegistrationPeriod",
                table: "Domains",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<decimal>(
                name: "RecurringAmount",
                table: "Domains",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "PriceCurrency",
                table: "Domains",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "USD");

            migrationBuilder.AlterColumn<decimal>(
                name: "FirstPaymentAmount",
                table: "Domains",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RegistrarModule",
                table: "Domains",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Domains",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ClientServices",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "RecurringAmount",
                table: "ClientServices",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "ClientServices",
                type: "character varying(1415)",
                maxLength: 1415,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FirstPaymentAmount",
                table: "ClientServices",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<int>(
                name: "FirstInvoiceId",
                table: "ClientServices",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "ClientServices",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Clients",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<string>(
                name: "TwoFactorSecret",
                table: "AspNetUsers",
                type: "character varying(735)",
                maxLength: 735,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingTwoFactorSecret",
                table: "AspNetUsers",
                type: "character varying(735)",
                maxLength: 735,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ClientId",
                table: "ActivityLogs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobRuns",
                columns: table => new
                {
                    JobName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LeaseOwner = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LeaseUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastStartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSucceededAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobRuns", x => x.JobName);
                });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Language", "Subject" },
                values: new object[] { "en", "Welcome to {{ site.name }}!" });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Body", "Language" },
                values: new object[] { "<p>Your invoice for {{invoice.total}} {{invoice.currency}} is ready.</p>", "en" });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 3,
                column: "Language",
                value: "en");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 4,
                column: "Language",
                value: "en");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 5,
                column: "Language",
                value: "en");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 6,
                column: "Language",
                value: "en");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 7,
                column: "Language",
                value: "en");

            // Domain names are compared and stored in lower case; the guard above made sure no two
            // rows collapse into one.
            migrationBuilder.Sql(
                """
                UPDATE "Domains" SET "Name" = lower("Name") WHERE "Name" <> lower("Name");
                """);

            // Billing cycles are written as the enum member name; legacy spellings become canonical so
            // SQL comparisons against the enum match every row.
            migrationBuilder.Sql(
                """
                UPDATE "ClientServices"
                SET "BillingCycle" = CASE WHEN lower(btrim("BillingCycle")) = 'monthly' THEN 'Monthly' ELSE 'Annual' END
                WHERE "BillingCycle" NOT IN ('Monthly', 'Annual');

                UPDATE "OrderItems"
                SET "BillingCycle" = CASE WHEN lower(btrim("BillingCycle")) = 'monthly' THEN 'Monthly' ELSE 'Annual' END
                WHERE "BillingCycle" NOT IN ('Monthly', 'Annual');
                """);

            // Existing domains were handled by their TLD's configured registrar, so that module is
            // recorded, picking the longest matching suffix the way the registrar resolver does. A
            // domain whose TLD has no configuration stays NULL and keeps following the default.
            migrationBuilder.Sql(
                """
                UPDATE "Domains" d
                SET "RegistrarModule" = (
                    SELECT t."RegistrarModule"
                    FROM "TldConfigs" t
                    WHERE d."Name" = t."Tld" OR d."Name" LIKE '%.' || t."Tld"
                    ORDER BY length(t."Tld") DESC
                    LIMIT 1)
                WHERE d."RegistrarModule" IS NULL;
                """);

            // The send log keeps a closed failure value, never the relay's own text. A failed send keeps
            // its failure as Unknown, a successful one carries none. Existing templates are English:
            // the new Language column's default already gave every existing row "en".
            migrationBuilder.Sql(
                """
                UPDATE "EmailLogs"
                SET "Error" = CASE WHEN "Success" THEN NULL ELSE 'Unknown' END
                WHERE "Error" IS NOT NULL
                  AND ("Success" OR "Error" NOT IN (
                      'Unknown', 'RelayUnreachable', 'Timeout', 'SecureConnectionFailed',
                      'AuthenticationRejected', 'RecipientRejected', 'SenderRejected', 'MessageRejected'));
                """);

            // Rows pointing at a parent that no longer exists would stop the foreign keys below. A
            // reference that may be empty is cleared (an invoice that is gone leaves its billable item
            // to be invoiced again); a row that cannot live without its parent is removed with it.
            migrationBuilder.Sql(
                """
                UPDATE "Transactions" r SET "InvoiceId" = NULL
                WHERE r."InvoiceId" IS NOT NULL AND NOT EXISTS (SELECT 1 FROM "Invoices" p WHERE p."Id" = r."InvoiceId");

                UPDATE "Orders" r SET "InvoiceId" = NULL
                WHERE r."InvoiceId" IS NOT NULL AND NOT EXISTS (SELECT 1 FROM "Invoices" p WHERE p."Id" = r."InvoiceId");

                UPDATE "BillableItems" r SET "InvoiceId" = NULL, "IsInvoiced" = false
                WHERE r."InvoiceId" IS NOT NULL AND NOT EXISTS (SELECT 1 FROM "Invoices" p WHERE p."Id" = r."InvoiceId");

                UPDATE "Domains" r SET "LinkedServiceId" = NULL
                WHERE r."LinkedServiceId" IS NOT NULL AND NOT EXISTS (SELECT 1 FROM "ClientServices" p WHERE p."Id" = r."LinkedServiceId");

                UPDATE "Tickets" r SET "DepartmentId" = NULL
                WHERE r."DepartmentId" IS NOT NULL AND NOT EXISTS (SELECT 1 FROM "Departments" p WHERE p."Id" = r."DepartmentId");

                UPDATE "Slides" r SET "ProductId" = NULL
                WHERE r."ProductId" IS NOT NULL AND NOT EXISTS (SELECT 1 FROM "Products" p WHERE p."Id" = r."ProductId");

                UPDATE "ActivityLogs" r SET "ClientId" = NULL
                WHERE r."ClientId" IS NOT NULL AND NOT EXISTS (SELECT 1 FROM "Clients" p WHERE p."Id" = r."ClientId");

                DELETE FROM "CancellationRequests" r
                WHERE NOT EXISTS (SELECT 1 FROM "ClientServices" p WHERE p."Id" = r."ServiceId");

                DELETE FROM "MigrationLogs" r
                WHERE NOT EXISTS (SELECT 1 FROM "MigrationJobs" p WHERE p."Id" = r."JobId");
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ClientId_Date",
                table: "Transactions",
                columns: new[] { "ClientId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Date",
                table: "Transactions",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_InvoiceId",
                table: "Transactions",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionId",
                table: "Transactions",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ClientId_CreatedAt",
                table: "Tickets",
                columns: new[] { "ClientId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_DepartmentId",
                table: "Tickets",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Status_CreatedAt",
                table: "Tickets",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Slides_ProductId",
                table: "Slides",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_ClientId",
                table: "Quotes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_PredefinedReplyCategories_ParentCategoryId",
                table: "PredefinedReplyCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ClientId",
                table: "Orders",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_InvoiceId",
                table: "Orders",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_CreatedAt",
                table: "Orders",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_KbCategories_ParentCategoryId",
                table: "KbCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ClientId",
                table: "Invoices",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ClientId_ExternalId",
                table: "Invoices",
                columns: new[] { "ClientId", "ExternalId" },
                unique: true,
                filter: "\"ExternalId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CreatedAt",
                table: "Invoices",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_GatewayOrderId",
                table: "Invoices",
                column: "GatewayOrderId",
                filter: "\"GatewayOrderId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Status_DueDate",
                table: "Invoices",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Status_PaidAt",
                table: "Invoices",
                columns: new[] { "Status", "PaidAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Slug_Language",
                table: "EmailTemplates",
                columns: new[] { "Slug", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_To_SentAt",
                table: "EmailLogs",
                columns: new[] { "To", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Domains_LinkedServiceId",
                table: "Domains",
                column: "LinkedServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Domains_Status_ExpiresAt",
                table: "Domains",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DomainReminders_DomainId_ReminderType_SentAt",
                table: "DomainReminders",
                columns: new[] { "DomainId", "ReminderType", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientServices_ClientId",
                table: "ClientServices",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientServices_ProductId",
                table: "ClientServices",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientServices_Status_NextRenewalAt",
                table: "ClientServices",
                columns: new[] { "Status", "NextRenewalAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CancellationRequests_ServiceId",
                table: "CancellationRequests",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BillableItems_ClientId",
                table: "BillableItems",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_BillableItems_InvoiceId",
                table: "BillableItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_ClientId_CreatedAt",
                table: "ActivityLogs",
                columns: new[] { "ClientId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLogs_Clients_ClientId",
                table: "ActivityLogs",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BillableItems_Clients_ClientId",
                table: "BillableItems",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BillableItems_Invoices_InvoiceId",
                table: "BillableItems",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CancellationRequests_ClientServices_ServiceId",
                table: "CancellationRequests",
                column: "ServiceId",
                principalTable: "ClientServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientServices_Clients_ClientId",
                table: "ClientServices",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientServices_Products_ProductId",
                table: "ClientServices",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Domains_ClientServices_LinkedServiceId",
                table: "Domains",
                column: "LinkedServiceId",
                principalTable: "ClientServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Domains_Clients_ClientId",
                table: "Domains",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadCategories_DownloadCategories_ParentCategoryId",
                table: "DownloadCategories",
                column: "ParentCategoryId",
                principalTable: "DownloadCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Downloads_DownloadCategories_CategoryId",
                table: "Downloads",
                column: "CategoryId",
                principalTable: "DownloadCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Clients_ClientId",
                table: "Invoices",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KbCategories_KbCategories_ParentCategoryId",
                table: "KbCategories",
                column: "ParentCategoryId",
                principalTable: "KbCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MigrationLogs_MigrationJobs_JobId",
                table: "MigrationLogs",
                column: "JobId",
                principalTable: "MigrationJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Clients_ClientId",
                table: "Orders",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Invoices_InvoiceId",
                table: "Orders",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PredefinedReplies_PredefinedReplyCategories_CategoryId",
                table: "PredefinedReplies",
                column: "CategoryId",
                principalTable: "PredefinedReplyCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PredefinedReplyCategories_ParentCategoryId",
                table: "PredefinedReplyCategories",
                column: "ParentCategoryId",
                principalTable: "PredefinedReplyCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_Clients_ClientId",
                table: "Quotes",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Slides_Products_ProductId",
                table: "Slides",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Clients_ClientId",
                table: "Tickets",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Departments_DepartmentId",
                table: "Tickets",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Clients_ClientId",
                table: "Transactions",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Invoices_InvoiceId",
                table: "Transactions",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLogs_Clients_ClientId",
                table: "ActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_BillableItems_Clients_ClientId",
                table: "BillableItems");

            migrationBuilder.DropForeignKey(
                name: "FK_BillableItems_Invoices_InvoiceId",
                table: "BillableItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CancellationRequests_ClientServices_ServiceId",
                table: "CancellationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientServices_Clients_ClientId",
                table: "ClientServices");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientServices_Products_ProductId",
                table: "ClientServices");

            migrationBuilder.DropForeignKey(
                name: "FK_Domains_ClientServices_LinkedServiceId",
                table: "Domains");

            migrationBuilder.DropForeignKey(
                name: "FK_Domains_Clients_ClientId",
                table: "Domains");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadCategories_DownloadCategories_ParentCategoryId",
                table: "DownloadCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Downloads_DownloadCategories_CategoryId",
                table: "Downloads");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Clients_ClientId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_KbCategories_KbCategories_ParentCategoryId",
                table: "KbCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_MigrationLogs_MigrationJobs_JobId",
                table: "MigrationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Clients_ClientId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Invoices_InvoiceId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_PredefinedReplies_PredefinedReplyCategories_CategoryId",
                table: "PredefinedReplies");

            migrationBuilder.DropForeignKey(
                name: "FK_PredefinedReplyCategories_ParentCategoryId",
                table: "PredefinedReplyCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Clients_ClientId",
                table: "Quotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Slides_Products_ProductId",
                table: "Slides");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Clients_ClientId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Departments_DepartmentId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Clients_ClientId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Invoices_InvoiceId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "JobRuns");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ClientId_Date",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_Date",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_InvoiceId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TransactionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ClientId_CreatedAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_DepartmentId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_Status_CreatedAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Slides_ProductId",
                table: "Slides");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_ClientId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_PredefinedReplyCategories_ParentCategoryId",
                table: "PredefinedReplyCategories");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ClientId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_InvoiceId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Status_CreatedAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_KbCategories_ParentCategoryId",
                table: "KbCategories");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_ClientId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_ClientId_ExternalId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_CreatedAt",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_GatewayOrderId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_Status_DueDate",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_Status_PaidAt",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_EmailTemplates_Slug_Language",
                table: "EmailTemplates");

            migrationBuilder.DropIndex(
                name: "IX_EmailLogs_To_SentAt",
                table: "EmailLogs");

            migrationBuilder.DropIndex(
                name: "IX_Domains_LinkedServiceId",
                table: "Domains");

            migrationBuilder.DropIndex(
                name: "IX_Domains_Status_ExpiresAt",
                table: "Domains");

            migrationBuilder.DropIndex(
                name: "IX_DomainReminders_DomainId_ReminderType_SentAt",
                table: "DomainReminders");

            migrationBuilder.DropIndex(
                name: "IX_ClientServices_ClientId",
                table: "ClientServices");

            migrationBuilder.DropIndex(
                name: "IX_ClientServices_ProductId",
                table: "ClientServices");

            migrationBuilder.DropIndex(
                name: "IX_ClientServices_Status_NextRenewalAt",
                table: "ClientServices");

            migrationBuilder.DropIndex(
                name: "IX_CancellationRequests_ServiceId",
                table: "CancellationRequests");

            migrationBuilder.DropIndex(
                name: "IX_BillableItems_ClientId",
                table: "BillableItems");

            migrationBuilder.DropIndex(
                name: "IX_BillableItems_InvoiceId",
                table: "BillableItems");

            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_ClientId_CreatedAt",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Slides");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "ServerLocations");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SkipReason",
                table: "MigrationLogs");

            migrationBuilder.DropColumn(
                name: "AnnouncementsFailed",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "ClientsFailed",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "ContactsFailed",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "DomainsFailed",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "DownloadsFailed",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "InvoicesFailed",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "KnowledgebaseFailed",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "LastHeartbeatAt",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "NetworkIssuesFailed",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "OrdersFailed",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "ProductsFailed",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "QuotesFailed",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "ServicesFailed",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "TicketRepliesFailed",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "TicketsFailed",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "TransactionsFailed",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "MigrationJobs");

            migrationBuilder.DropColumn(
                name: "GatewayCheckCount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "GatewayCheckedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "GatewayConcludedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "GatewayStalledNotifiedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RefundStartedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Faqs");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "EmailTemplates");

            migrationBuilder.DropColumn(
                name: "RegistrarModule",
                table: "Domains");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Domains");

            migrationBuilder.DropColumn(
                name: "FirstInvoiceId",
                table: "ClientServices");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "ClientServices");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PendingTwoFactorSecret",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Features",
                table: "SlideTranslations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(8192)",
                oldMaxLength: 8192,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Settings",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10719)",
                oldMaxLength: 10719);

            migrationBuilder.AlterColumn<string>(
                name: "AgentPrivateKey",
                table: "ServerSshSettings",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(42719)",
                oldMaxLength: 42719,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Servers",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2719)",
                oldMaxLength: 2719,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApiToken",
                table: "Servers",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(5383)",
                oldMaxLength: 5383,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccessHash",
                table: "Servers",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10719)",
                oldMaxLength: 10719,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountPercent",
                table: "QuoteItems",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Website",
                table: "Products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Products",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Products",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "OrderItems",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<decimal>(
                name: "RecurringAmount",
                table: "OrderItems",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "FirstPaymentAmount",
                table: "OrderItems",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "Invoices",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RegistrationPeriod",
                table: "Domains",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "RecurringAmount",
                table: "Domains",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AlterColumn<string>(
                name: "PriceCurrency",
                table: "Domains",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "FirstPaymentAmount",
                table: "Domains",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ClientServices",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<decimal>(
                name: "RecurringAmount",
                table: "ClientServices",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "ClientServices",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1415)",
                oldMaxLength: 1415,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FirstPaymentAmount",
                table: "ClientServices",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AlterColumn<string>(
                name: "TwoFactorSecret",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(735)",
                oldMaxLength: 735,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ClientId",
                table: "ActivityLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "Subject",
                value: "Welcome to Innovayse!");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 2,
                column: "Body",
                value: "<p>Your invoice for {{invoice.total}} is ready.</p>");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ClientId",
                table: "Transactions",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ClientId",
                table: "Tickets",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Status",
                table: "Tickets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationLogs_JobId",
                table: "MigrationLogs",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Slug",
                table: "EmailTemplates",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DomainReminders_DomainId",
                table: "DomainReminders",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_ClientId",
                table: "ActivityLogs",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_CreatedAt",
                table: "ActivityLogs",
                column: "CreatedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_Downloads_DownloadCategories_CategoryId",
                table: "Downloads",
                column: "CategoryId",
                principalTable: "DownloadCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
