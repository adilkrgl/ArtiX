using Microsoft.EntityFrameworkCore.Migrations;

namespace ArtiX.Infrastructure.Migrations;

public partial class AddCustomerBasicFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Code",
            table: "Customers",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "Customers",
            type: "bit",
            nullable: false,
            defaultValue: true);

        migrationBuilder.AddColumn<string>(
            name: "Name",
            table: "Customers",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "TaxNumber",
            table: "Customers",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Code",
            table: "Customers");

        migrationBuilder.DropColumn(
            name: "IsActive",
            table: "Customers");

        migrationBuilder.DropColumn(
            name: "Name",
            table: "Customers");

        migrationBuilder.DropColumn(
            name: "TaxNumber",
            table: "Customers");
    }
}
