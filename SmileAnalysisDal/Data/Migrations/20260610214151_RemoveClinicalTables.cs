using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmileAnalysisDal.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveClinicalTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Classifications");

            migrationBuilder.DropTable(
                name: "GummySmileDiagnoses");

            migrationBuilder.DropTable(
                name: "Landmarks");

            migrationBuilder.DropTable(
                name: "Measurements");

            migrationBuilder.DropTable(
                name: "TreatmentPlans");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "NormReferences");

            migrationBuilder.DropTable(
                name: "CaseRecords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaseRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    CaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseRecords_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CaseRecords_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NormReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgeMax = table.Column<int>(type: "int", nullable: false),
                    AgeMin = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    IdealValue = table.Column<double>(type: "float", nullable: true),
                    MeasurementName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NormalMax = table.Column<double>(type: "float", nullable: false),
                    NormalMin = table.Column<double>(type: "float", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormReferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Classifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SmileArcType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Subtype = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classifications_CaseRecords_CaseId",
                        column: x => x.CaseId,
                        principalTable: "CaseRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GummySmileDiagnoses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseId = table.Column<int>(type: "int", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FactorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FactorType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PercentageContribution = table.Column<double>(type: "float", nullable: true),
                    QuantificationValue = table.Column<double>(type: "float", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GummySmileDiagnoses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GummySmileDiagnoses_CaseRecords_CaseId",
                        column: x => x.CaseId,
                        principalTable: "CaseRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Height = table.Column<int>(type: "int", nullable: true),
                    ImageType = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_CaseRecords_CaseId",
                        column: x => x.CaseId,
                        principalTable: "CaseRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    PrimaryDiagnosis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PriorityLevel = table.Column<int>(type: "int", nullable: true),
                    ProcedureCategory = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RecommendedProcedure = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentPlans_CaseRecords_CaseId",
                        column: x => x.CaseId,
                        principalTable: "CaseRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Measurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseId = table.Column<int>(type: "int", nullable: false),
                    NormId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deviation = table.Column<double>(type: "float", nullable: true),
                    IsNormal = table.Column<bool>(type: "bit", nullable: false),
                    MeasurementName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Value = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Measurements_CaseRecords_CaseId",
                        column: x => x.CaseId,
                        principalTable: "CaseRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Measurements_NormReferences_NormId",
                        column: x => x.NormId,
                        principalTable: "NormReferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Landmarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageId = table.Column<int>(type: "int", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LandmarkName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    XCoord = table.Column<double>(type: "float", nullable: false),
                    YCoord = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Landmarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Landmarks_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseRecords_DoctorId",
                table: "CaseRecords",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseRecords_PatientId",
                table: "CaseRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Classifications_CaseId",
                table: "Classifications",
                column: "CaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GummySmileDiagnoses_CaseId",
                table: "GummySmileDiagnoses",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_CaseId",
                table: "Images",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Landmarks_ImageId",
                table: "Landmarks",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_CaseId",
                table: "Measurements",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_NormId",
                table: "Measurements",
                column: "NormId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPlans_CaseId",
                table: "TreatmentPlans",
                column: "CaseId",
                unique: true);
        }
    }
}
