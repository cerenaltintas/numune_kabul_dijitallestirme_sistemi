using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NumuneKabul.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfIdToAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PdfDocumentId",
                table: "AuditLogs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_PdfDocumentId",
                table: "AuditLogs",
                column: "PdfDocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_PdfDocuments_PdfDocumentId",
                table: "AuditLogs",
                column: "PdfDocumentId",
                principalTable: "PdfDocuments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_PdfDocuments_PdfDocumentId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_PdfDocumentId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "PdfDocumentId",
                table: "AuditLogs");
        }
    }
}
