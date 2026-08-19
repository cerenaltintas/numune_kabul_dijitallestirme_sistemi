using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NumuneKabul.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConfidenceScores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ConfidenceScore",
                table: "PdfDocuments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageConfidence",
                table: "OcrResults",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "PdfDocuments");

            migrationBuilder.DropColumn(
                name: "AverageConfidence",
                table: "OcrResults");
        }
    }
}
