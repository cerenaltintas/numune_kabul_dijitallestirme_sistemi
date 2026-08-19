using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NumuneKabul.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeAuditLogPolymorphic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "EntityId",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewValues",
                table: "AuditLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldValues",
                table: "AuditLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "NewValues",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "OldValues",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "AuditLogs");

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
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
