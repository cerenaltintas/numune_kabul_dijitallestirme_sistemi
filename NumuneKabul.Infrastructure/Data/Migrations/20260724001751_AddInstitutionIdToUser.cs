using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NumuneKabul.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutionIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstitutionId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_InstitutionId",
                table: "Users",
                column: "InstitutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Institutions_InstitutionId",
                table: "Users",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Institutions_InstitutionId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_InstitutionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "Users");
        }
    }
}
