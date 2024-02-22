using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtCommissions.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommissionReports_Reports_ReportId",
                table: "CommissionReports");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_Reports_ReportId",
                table: "UserReports");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_UserReports_ReportId",
                table: "UserReports");

            migrationBuilder.DropIndex(
                name: "IX_CommissionReports_ReportId",
                table: "CommissionReports");

            migrationBuilder.DropColumn(
                name: "ReportId",
                table: "UserReports");

            migrationBuilder.DropColumn(
                name: "ReportId",
                table: "CommissionReports");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "UserReports",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<short>(
                name: "Status",
                table: "UserReports",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "CommissionReports",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<short>(
                name: "Status",
                table: "CommissionReports",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommissionTags",
                columns: table => new
                {
                    CommissionId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionTags", x => new { x.TagId, x.CommissionId });
                    table.ForeignKey(
                        name: "FK_CommissionTags_Commissions_CommissionId",
                        column: x => x.CommissionId,
                        principalTable: "Commissions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CommissionTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommissionTags_CommissionId",
                table: "CommissionTags",
                column: "CommissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommissionTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "UserReports");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "UserReports");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "CommissionReports");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CommissionReports");

            migrationBuilder.AddColumn<int>(
                name: "ReportId",
                table: "UserReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReportId",
                table: "CommissionReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReportId",
                table: "UserReports",
                column: "ReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissionReports_ReportId",
                table: "CommissionReports",
                column: "ReportId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CommissionReports_Reports_ReportId",
                table: "CommissionReports",
                column: "ReportId",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_Reports_ReportId",
                table: "UserReports",
                column: "ReportId",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
