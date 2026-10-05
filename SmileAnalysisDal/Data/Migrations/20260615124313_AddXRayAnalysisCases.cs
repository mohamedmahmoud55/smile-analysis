using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmileAnalysisDal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddXRayAnalysisCases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "XRayAnalysisCases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CvmStage = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    SkeletalClass = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LandmarkCount = table.Column<int>(type: "int", nullable: true),
                    ConfidencePercent = table.Column<int>(type: "int", nullable: false),
                    PanoramicImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PanoramicImageContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CephImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CephImageContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ResultsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XRayAnalysisCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_XRayAnalysisCases_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_XRayAnalysisCases_PatientId",
                table: "XRayAnalysisCases",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "XRayAnalysisCases");
        }
    }
}
