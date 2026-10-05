using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmileAnalysisDal.Data.Contexts;

#nullable disable

namespace SmileAnalysisDal.Data.Migrations;

[DbContext(typeof(SmileAnalysisDbContext))]
[Migration("20260613000000_AddGummySmileCases")]
public partial class AddGummySmileCases : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "GummySmileCases",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PatientId = table.Column<int>(type: "int", nullable: false),
                GsCaseId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                GsStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                DiagnosisSummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReportJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ReportPdf = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GummySmileCases", x => x.Id);
                table.ForeignKey(
                    name: "FK_GummySmileCases_Patients_PatientId",
                    column: x => x.PatientId,
                    principalTable: "Patients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_GummySmileCases_GsCaseId",
            table: "GummySmileCases",
            column: "GsCaseId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_GummySmileCases_PatientId",
            table: "GummySmileCases",
            column: "PatientId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "GummySmileCases");
    }
}
