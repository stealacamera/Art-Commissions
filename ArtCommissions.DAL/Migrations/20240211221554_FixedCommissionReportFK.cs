using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtCommissions.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixedCommissionReportFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommissionReports_AppUsers_ReportedCommissionId",
                table: "CommissionReports");

            migrationBuilder.AddForeignKey(
                name: "FK_CommissionReports_Commissions_ReportedCommissionId",
                table: "CommissionReports",
                column: "ReportedCommissionId",
                principalTable: "Commissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommissionReports_Commissions_ReportedCommissionId",
                table: "CommissionReports");

            migrationBuilder.AddForeignKey(
                name: "FK_CommissionReports_AppUsers_ReportedCommissionId",
                table: "CommissionReports",
                column: "ReportedCommissionId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
