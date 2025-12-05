using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtiX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "SalesOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OrderDate",
                table: "SalesOrders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "SalesOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceDate",
                table: "Invoices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "Invoices",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_CustomerId",
                table: "SalesOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Customers_CustomerId",
                table: "Invoices",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Customers_CustomerId",
                table: "SalesOrders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Customers_CustomerId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Customers_CustomerId",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_CustomerId",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "OrderDate",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "InvoiceDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "Invoices");
        }
    }
}
