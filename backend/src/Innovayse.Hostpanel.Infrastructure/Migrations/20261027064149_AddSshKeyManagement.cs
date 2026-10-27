using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Innovayse.Hostpanel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSshKeyManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServerSshSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServerId = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SshHostname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SshPort = table.Column<int>(type: "integer", nullable: false),
                    AgentHost = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AgentPort = table.Column<int>(type: "integer", nullable: false),
                    AgentUsername = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AgentPrivateKey = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    AgentHostKeyFingerprint = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConnectTimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    OperationTimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    IsHealthy = table.Column<bool>(type: "boolean", nullable: true),
                    LastHealthCheckAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerSshSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerSshSettings_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SshKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KeyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientServiceId = table.Column<int>(type: "integer", nullable: false),
                    ServerId = table.Column<int>(type: "integer", nullable: false),
                    AccountUsername = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PublicKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Fingerprint = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    SyncState = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastSyncError = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SshKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SshKeys_ClientServices_ClientServiceId",
                        column: x => x.ClientServiceId,
                        principalTable: "ClientServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServerSshSettings_ServerId",
                table: "ServerSshSettings",
                column: "ServerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SshKeys_ClientServiceId_Fingerprint",
                table: "SshKeys",
                columns: new[] { "ClientServiceId", "Fingerprint" },
                unique: true,
                filter: "\"SyncState\" <> 'Revoked'");

            migrationBuilder.CreateIndex(
                name: "IX_SshKeys_KeyId",
                table: "SshKeys",
                column: "KeyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SshKeys_ServerId",
                table: "SshKeys",
                column: "ServerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerSshSettings");

            migrationBuilder.DropTable(
                name: "SshKeys");
        }
    }
}
