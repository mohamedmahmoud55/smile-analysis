using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmileAnalysisDal.Data.Contexts;

#nullable disable

namespace SmileAnalysisDal.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(SmileAnalysisDbContext))]
    [Migration("20260415120000_DoctorStaffEmailPhoneBirthDate")]
    public partial class DoctorStaffEmailPhoneBirthDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDate",
                table: "Doctors",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Doctors",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Doctors",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDate",
                table: "Staff",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Staff",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Staff",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "BirthDate", table: "Doctors");
            migrationBuilder.DropColumn(name: "Email", table: "Doctors");
            migrationBuilder.DropColumn(name: "Phone", table: "Doctors");
            migrationBuilder.DropColumn(name: "BirthDate", table: "Staff");
            migrationBuilder.DropColumn(name: "Email", table: "Staff");
            migrationBuilder.DropColumn(name: "Phone", table: "Staff");
        }
    }
}
