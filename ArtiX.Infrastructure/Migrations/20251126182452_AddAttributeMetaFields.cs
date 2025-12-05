using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtiX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttributeMetaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomValue",
                table: "ProductAttributeValues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "AttributeValues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DataType",
                table: "AttributeDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AttributeDefinitions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFilterable",
                table: "AttributeDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "AttributeDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVariant",
                table: "AttributeDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "AttributeDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomValue",
                table: "ProductAttributeValues");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "AttributeValues");

            migrationBuilder.DropColumn(
                name: "DataType",
                table: "AttributeDefinitions");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AttributeDefinitions");

            migrationBuilder.DropColumn(
                name: "IsFilterable",
                table: "AttributeDefinitions");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "AttributeDefinitions");

            migrationBuilder.DropColumn(
                name: "IsVariant",
                table: "AttributeDefinitions");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "AttributeDefinitions");
        }
    }
}
