using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NumuneKabul.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Institutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InstitutionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormTemplates_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PdfDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InstitutionId = table.Column<int>(type: "INTEGER", nullable: false),
                    TemplateId = table.Column<int>(type: "INTEGER", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    UploadDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PageCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PdfDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PdfDocuments_FormTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "FormTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PdfDocuments_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TemplateFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Regex = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Required = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    OrderNo = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateFields_FormTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "FormTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExtractedFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PdfDocumentId = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RawValue = table.Column<string>(type: "TEXT", nullable: true),
                    CorrectedValue = table.Column<string>(type: "TEXT", nullable: true),
                    Confidence = table.Column<decimal>(type: "TEXT", nullable: false),
                    PageNo = table.Column<int>(type: "INTEGER", nullable: false),
                    X = table.Column<int>(type: "INTEGER", nullable: false),
                    Y = table.Column<int>(type: "INTEGER", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractedFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtractedFields_PdfDocuments_PdfDocumentId",
                        column: x => x.PdfDocumentId,
                        principalTable: "PdfDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PdfDocumentId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastAttemptDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntegrationJobs_PdfDocuments_PdfDocumentId",
                        column: x => x.PdfDocumentId,
                        principalTable: "PdfDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OcrResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PdfDocumentId = table.Column<int>(type: "INTEGER", nullable: false),
                    RawText = table.Column<string>(type: "TEXT", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OcrResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OcrResults_PdfDocuments_PdfDocumentId",
                        column: x => x.PdfDocumentId,
                        principalTable: "PdfDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "XmlArchives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PdfDocumentId = table.Column<int>(type: "INTEGER", nullable: false),
                    XmlContent = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XmlArchives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_XmlArchives_PdfDocuments_PdfDocumentId",
                        column: x => x.PdfDocumentId,
                        principalTable: "PdfDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractedFields_PdfDocumentId",
                table: "ExtractedFields",
                column: "PdfDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplates_InstitutionId",
                table: "FormTemplates",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationJobs_PdfDocumentId",
                table: "IntegrationJobs",
                column: "PdfDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_OcrResults_PdfDocumentId",
                table: "OcrResults",
                column: "PdfDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PdfDocuments_InstitutionId",
                table: "PdfDocuments",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_PdfDocuments_TemplateId",
                table: "PdfDocuments",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateFields_TemplateId",
                table: "TemplateFields",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XmlArchives_PdfDocumentId",
                table: "XmlArchives",
                column: "PdfDocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ExtractedFields");

            migrationBuilder.DropTable(
                name: "IntegrationJobs");

            migrationBuilder.DropTable(
                name: "OcrResults");

            migrationBuilder.DropTable(
                name: "TemplateFields");

            migrationBuilder.DropTable(
                name: "XmlArchives");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "PdfDocuments");

            migrationBuilder.DropTable(
                name: "FormTemplates");

            migrationBuilder.DropTable(
                name: "Institutions");
        }
    }
}
