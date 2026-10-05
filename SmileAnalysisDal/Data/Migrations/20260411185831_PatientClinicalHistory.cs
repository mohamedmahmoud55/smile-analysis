using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmileAnalysisDal.Data.Migrations
{
    /// <inheritdoc />
    public partial class PatientClinicalHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientClinicalHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ChronicDiseases = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CurrentMedications = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PreviousSurgeries = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    HeartOrBleedingConditions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    LastDentalVisit = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentPainDetails = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PreviousDentalTreatments = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    GumProblems = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    OralHabits = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientClinicalHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientClinicalHistories_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientClinicalHistories_PatientId",
                table: "PatientClinicalHistories",
                column: "PatientId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientClinicalHistories");
        }
    }
}
