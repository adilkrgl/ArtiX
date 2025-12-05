using Microsoft.EntityFrameworkCore.Migrations;

namespace ArtiX.Infrastructure.Migrations;

public partial class AddLineCustomDescriptionAndNote : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<Guid>(
            name: "ProductId",
            table: "SalesOrderLines",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier");

        migrationBuilder.AlterColumn<Guid>(
            name: "ProductId",
            table: "QuotationLines",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier");

        migrationBuilder.AlterColumn<Guid>(
            name: "ProductId",
            table: "InvoiceLines",
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
            defaultValue: Guid.Empty,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "ProductId",
            table: "QuotationLines",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: Guid.Empty,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "ProductId",
            table: "InvoiceLines",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: Guid.Empty,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);
    }
}
