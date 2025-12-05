using Microsoft.EntityFrameworkCore.Migrations;

namespace ArtiX.Infrastructure.Migrations;

public partial class AddInvoiceNumber : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "InvoiceNumber",
            table: "Invoices",
            type: "nvarchar(16)",
            maxLength: 16,
            nullable: true);

        migrationBuilder.Sql(@"
            WITH NumberedInvoices AS (
                SELECT Id, InvoiceDate, ROW_NUMBER() OVER (ORDER BY Id) AS rn
                FROM Invoices
            )
            UPDATE ni
            SET InvoiceNumber = CONCAT(
                FORMAT(ISNULL(ni.InvoiceDate, GETUTCDATE()), 'yyyy'),
                RIGHT(CONCAT('00000', ni.rn), 5)
            )
            FROM NumberedInvoices ni;
        ");

        migrationBuilder.AlterColumn<string>(
            name: "InvoiceNumber",
            table: "Invoices",
            type: "nvarchar(16)",
            maxLength: 16,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(16)",
            oldMaxLength: 16,
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Invoices_InvoiceNumber",
            table: "Invoices",
            column: "InvoiceNumber",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Invoices_InvoiceNumber",
            table: "Invoices");

        migrationBuilder.DropColumn(
            name: "InvoiceNumber",
            table: "Invoices");
    }
}
