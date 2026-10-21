using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innovayse.Hostpanel.Infrastructure.Migrations
{
    /// <summary>
    /// Adds the operator-controlled <c>IsFeatured</c> (recommended plan) and <c>SortOrder</c>
    /// (position inside the group) columns to <c>products</c>. Additive: existing rows keep
    /// <c>false</c> and <c>0</c>, so the storefront ordering is unchanged until an operator sets them.
    /// </summary>
    public partial class AddProductFeaturedAndSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "products");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "products");
        }
    }
}
