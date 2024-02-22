using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtCommissions.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedUserStripeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppUsers_StripeId",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "StripeId",
                table: "AppUsers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Invoices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 2, 7, 11, 45, 46, 655, DateTimeKind.Local).AddTicks(205),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 2, 6, 18, 39, 16, 916, DateTimeKind.Local).AddTicks(3467));

            migrationBuilder.AddColumn<string>(
                name: "StripeAccountId",
                table: "AppUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                table: "AppUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripeAccountId",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "StripeCustomerId",
                table: "AppUsers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Invoices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 2, 6, 18, 39, 16, 916, DateTimeKind.Local).AddTicks(3467),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 2, 7, 11, 45, 46, 655, DateTimeKind.Local).AddTicks(205));

            migrationBuilder.AddColumn<string>(
                name: "StripeId",
                table: "AppUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_StripeId",
                table: "AppUsers",
                column: "StripeId",
                unique: true);
        }
    }
}
