using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtiX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLineCustomDescriptionAndNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "SalesOrderLines",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "CustomDescription",
                table: "SalesOrderLines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineNote",
                table: "SalesOrderLines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "QuotationLines",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "CustomDescription",
                table: "QuotationLines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineNote",
                table: "QuotationLines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "InvoiceLines",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "CustomDescription",
                table: "InvoiceLines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineNote",
                table: "InvoiceLines",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomDescription",
                table: "SalesOrderLines");

            migrationBuilder.DropColumn(
                name: "LineNote",
                table: "SalesOrderLines");

            migrationBuilder.DropColumn(
                name: "CustomDescription",
                table: "QuotationLines");

            migrationBuilder.DropColumn(
                name: "LineNote",
                table: "QuotationLines");

            migrationBuilder.DropColumn(
                name: "CustomDescription",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "LineNote",
                table: "InvoiceLines");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "SalesOrderLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "QuotationLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "InvoiceLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
