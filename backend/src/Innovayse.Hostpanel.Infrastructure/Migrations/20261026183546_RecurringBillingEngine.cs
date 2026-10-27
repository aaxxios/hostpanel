using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Innovayse.Hostpanel.Infrastructure.Migrations
{
    /// <summary>Issue #67: renewal lines (with their claim and applied markers), reminders, auto-renew and suspension source.</summary>
    public partial class RecurringBillingEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegisteredDomainId",
                table: "InvoiceItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RenewalAppliedAt",
                table: "InvoiceItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RenewalAttemptAt",
                table: "InvoiceItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RenewalPeriodEnd",
                table: "InvoiceItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RenewalPeriodStart",
                table: "InvoiceItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoRenew",
                table: "ClientServices",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionSource",
                table: "ClientServices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InvoiceReminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceId = table.Column<int>(type: "integer", nullable: false),
                    DaysBefore = table.Column<int>(type: "integer", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceReminders_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_ClientServiceId_RenewalPeriodStart",
                table: "InvoiceItems",
                columns: new[] { "ClientServiceId", "RenewalPeriodStart" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_PendingRenewal",
                table: "InvoiceItems",
                column: "RenewalAppliedAt",
                filter: "\"RenewalAppliedAt\" IS NULL AND \"RenewalPeriodStart\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_RegisteredDomainId_RenewalPeriodStart",
                table: "InvoiceItems",
                columns: new[] { "RegisteredDomainId", "RenewalPeriodStart" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceReminders_InvoiceId_DaysBefore",
                table: "InvoiceReminders",
                columns: new[] { "InvoiceId", "DaysBefore" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceReminders");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceItems_ClientServiceId_RenewalPeriodStart",
                table: "InvoiceItems");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceItems_PendingRenewal",
                table: "InvoiceItems");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceItems_RegisteredDomainId_RenewalPeriodStart",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "RegisteredDomainId",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "RenewalAppliedAt",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "RenewalAttemptAt",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "RenewalPeriodEnd",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "RenewalPeriodStart",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "AutoRenew",
                table: "ClientServices");

            migrationBuilder.DropColumn(
                name: "SuspensionSource",
                table: "ClientServices");
        }
    }
}
