using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtCommissions.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeleteBehaviour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommissionTags_Commissions_CommissionId",
                table: "CommissionTags");

            migrationBuilder.DropForeignKey(
                name: "FK_CommissionTags_Tags_TagId",
                table: "CommissionTags");

            migrationBuilder.AddForeignKey(
                name: "FK_CommissionTags_Commissions_CommissionId",
                table: "CommissionTags",
                column: "CommissionId",
                principalTable: "Commissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommissionTags_Tags_TagId",
                table: "CommissionTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommissionTags_Commissions_CommissionId",
                table: "CommissionTags");

            migrationBuilder.DropForeignKey(
                name: "FK_CommissionTags_Tags_TagId",
                table: "CommissionTags");

            migrationBuilder.AddForeignKey(
                name: "FK_CommissionTags_Commissions_CommissionId",
                table: "CommissionTags",
                column: "CommissionId",
                principalTable: "Commissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommissionTags_Tags_TagId",
                table: "CommissionTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id");
        }
    }
}
