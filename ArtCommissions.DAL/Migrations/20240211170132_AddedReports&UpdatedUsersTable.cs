using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtCommissions.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedReportsUpdatedUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinalImages_Orders_OrderId",
                table: "FinalImages");

            migrationBuilder.DropIndex(
                name: "IX_FinalImages_OrderId",
                table: "FinalImages");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "AppUsers");

            migrationBuilder.AddColumn<int>(
                name: "NrSuspensionStrikes",
                table: "AppUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommissionReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    ReportedCommissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionReports_AppUsers_ReportedCommissionId",
                        column: x => x.ReportedCommissionId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommissionReports_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    ReportedUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserReports_AppUsers_ReportedUserId",
                        column: x => x.ReportedUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserReports_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinalImages_OrderId",
                table: "FinalImages",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissionReports_ReportedCommissionId",
                table: "CommissionReports",
                column: "ReportedCommissionId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionReports_ReportId",
                table: "CommissionReports",
                column: "ReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReportedUserId",
                table: "UserReports",
                column: "ReportedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReportId",
                table: "UserReports",
                column: "ReportId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FinalImages_Orders_OrderId",
                table: "FinalImages",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinalImages_Orders_OrderId",
                table: "FinalImages");

            migrationBuilder.DropTable(
                name: "CommissionReports");

            migrationBuilder.DropTable(
                name: "UserReports");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_FinalImages_OrderId",
                table: "FinalImages");

            migrationBuilder.DropColumn(
                name: "NrSuspensionStrikes",
                table: "AppUsers");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "AppUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinalImages_OrderId",
                table: "FinalImages",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinalImages_Orders_OrderId",
                table: "FinalImages",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
