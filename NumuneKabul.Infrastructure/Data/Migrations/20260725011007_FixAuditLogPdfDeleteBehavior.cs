using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NumuneKabul.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixAuditLogPdfDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_PdfDocuments_PdfDocumentId",
                table: "AuditLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_PdfDocuments_PdfDocumentId",
                table: "AuditLogs",
                column: "PdfDocumentId",
                principalTable: "PdfDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_PdfDocuments_PdfDocumentId",
                table: "AuditLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_PdfDocuments_PdfDocumentId",
                table: "AuditLogs",
                column: "PdfDocumentId",
                principalTable: "PdfDocuments",
                principalColumn: "Id");
        }
    }
}
