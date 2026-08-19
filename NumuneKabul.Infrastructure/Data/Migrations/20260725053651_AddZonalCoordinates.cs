using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NumuneKabul.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddZonalCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "TemplateFields",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Psm",
                table: "TemplateFields",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "TemplateFields",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "X",
                table: "TemplateFields",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Y",
                table: "TemplateFields",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "TemplateFields");

            migrationBuilder.DropColumn(
                name: "Psm",
                table: "TemplateFields");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "TemplateFields");

            migrationBuilder.DropColumn(
                name: "X",
                table: "TemplateFields");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "TemplateFields");
        }
    }
}
