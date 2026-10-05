using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmileAnalysisDal.Data.Contexts;

#nullable disable

namespace SmileAnalysisDal.Data.Migrations;

[DbContext(typeof(SmileAnalysisDbContext))]
[Migration("20260613020000_GummySmileFullPersistence")]
public partial class GummySmileFullPersistence : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CachedAssetsJson",
            table: "GummySmileCases",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "CaseSnapshotJson",
            table: "GummySmileCases",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "RestImage",
            table: "GummySmileCases",
            type: "varbinary(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "RestImageContentType",
            table: "GummySmileCases",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "SmileImage",
            table: "GummySmileCases",
            type: "varbinary(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SmileImageContentType",
            table: "GummySmileCases",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "CachedAssetsJson", table: "GummySmileCases");
        migrationBuilder.DropColumn(name: "CaseSnapshotJson", table: "GummySmileCases");
        migrationBuilder.DropColumn(name: "RestImage", table: "GummySmileCases");
        migrationBuilder.DropColumn(name: "RestImageContentType", table: "GummySmileCases");
        migrationBuilder.DropColumn(name: "SmileImage", table: "GummySmileCases");
        migrationBuilder.DropColumn(name: "SmileImageContentType", table: "GummySmileCases");
    }
}
