using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innovayse.Hostpanel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderNumberSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "OrderNumberSequence");

            // Continue the numbering where the old max(Id) + 1 scheme left off: the first number
            // handed out is the one that scheme would have produced next, so existing orders keep
            // their numbers and the series carries on without a jump or a repeat.
            migrationBuilder.Sql(
                "SELECT setval('\"OrderNumberSequence\"', COALESCE(MAX(\"Id\"), 0) + 1, false) FROM \"Orders\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "OrderNumberSequence");
        }
    }
}
