using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmileAnalysisDal.Data.Contexts;

#nullable disable

namespace SmileAnalysisDal.Data.Migrations;

[DbContext(typeof(SmileAnalysisDbContext))]
[Migration("20260613010000_GummySmileAuditDrafts")]
public partial class GummySmileAuditDrafts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ClinicalDraftJson",
            table: "GummySmileCases",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ClinicalRequestJson",
            table: "GummySmileCases",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "OverridesRequestJson",
            table: "GummySmileCases",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ClinicalDraftJson", table: "GummySmileCases");
        migrationBuilder.DropColumn(name: "ClinicalRequestJson", table: "GummySmileCases");
        migrationBuilder.DropColumn(name: "OverridesRequestJson", table: "GummySmileCases");
    }
}
