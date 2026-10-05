using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmileAnalysisDal.Data.Contexts;

#nullable disable

namespace SmileAnalysisDal.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(SmileAnalysisDbContext))]
    [Migration("20260415163000_DoctorApplicationUserLink")]
    public partial class DoctorApplicationUserLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Doctors",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_ApplicationUserId",
                table: "Doctors",
                column: "ApplicationUserId",
                unique: true,
                filter: "[ApplicationUserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_AspNetUsers_ApplicationUserId",
                table: "Doctors",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_AspNetUsers_ApplicationUserId",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_ApplicationUserId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Doctors");
        }
    }
}
