using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmileAnalysisDal.Data.Contexts;

#nullable disable

namespace SmileAnalysisDal.Data.Migrations;

[DbContext(typeof(SmileAnalysisDbContext))]
[Migration("20260415220000_FlexiblePatientBilling")]
public partial class FlexiblePatientBilling : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Payments_Appointments_AppointmentId",
            table: "Payments");

        migrationBuilder.AlterColumn<int>(
            name: "AppointmentId",
            table: "Payments",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AddForeignKey(
            name: "FK_Payments_Appointments_AppointmentId",
            table: "Payments",
            column: "AppointmentId",
            principalTable: "Appointments",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.CreateTable(
            name: "PatientBillingAccounts",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PatientId = table.Column<int>(type: "int", nullable: false),
                BaseTreatmentCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                AdditionalCosts = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                DiscountType = table.Column<int>(type: "int", nullable: false),
                DiscountPercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                DiscountFixedAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PatientBillingAccounts", x => x.Id);
                table.ForeignKey(
                    name: "FK_PatientBillingAccounts_Patients_PatientId",
                    column: x => x.PatientId,
                    principalTable: "Patients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PatientBillingAccounts_PatientId",
            table: "PatientBillingAccounts",
            column: "PatientId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PatientBillingAccounts");

        migrationBuilder.DropForeignKey(
            name: "FK_Payments_Appointments_AppointmentId",
            table: "Payments");

        migrationBuilder.AlterColumn<int>(
            name: "AppointmentId",
            table: "Payments",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Payments_Appointments_AppointmentId",
            table: "Payments",
            column: "AppointmentId",
            principalTable: "Appointments",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}
