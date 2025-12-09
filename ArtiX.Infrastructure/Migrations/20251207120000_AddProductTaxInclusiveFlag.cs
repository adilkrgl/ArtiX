using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtiX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductTaxInclusiveFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTaxInclusive",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTaxInclusive",
                table: "InvoiceLines",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTaxInclusive",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsTaxInclusive",
                table: "InvoiceLines");
        }
    }
}
