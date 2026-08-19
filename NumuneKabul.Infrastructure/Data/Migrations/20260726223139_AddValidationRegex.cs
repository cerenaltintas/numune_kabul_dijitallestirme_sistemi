using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NumuneKabul.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddValidationRegex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ValidationRegex",
                table: "TemplateFields",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValidationRegex",
                table: "TemplateFields");
        }
    }
}
