using Microsoft.EntityFrameworkCore.Migrations;

namespace ArtiX.Infrastructure.Migrations;

public partial class AddProductBasicFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "ProductTypeId",
            table: "Products",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Name",
            table: "Products",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "Sku",
            table: "Products",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Barcode",
            table: "Products",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "Products",
            type: "bit",
            nullable: false,
            defaultValue: true);

        migrationBuilder.CreateIndex(
            name: "IX_Products_ProductTypeId",
            table: "Products",
            column: "ProductTypeId");

        migrationBuilder.AddForeignKey(
            name: "FK_Products_ProductTypes_ProductTypeId",
            table: "Products",
            column: "ProductTypeId",
            principalTable: "ProductTypes",
            principalColumn: "Id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Products_ProductTypes_ProductTypeId",
            table: "Products");

        migrationBuilder.DropIndex(
            name: "IX_Products_ProductTypeId",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "ProductTypeId",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "Name",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "Sku",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "Barcode",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "IsActive",
            table: "Products");
    }
}
