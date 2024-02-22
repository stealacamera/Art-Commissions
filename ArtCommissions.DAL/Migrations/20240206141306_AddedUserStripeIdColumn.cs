using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtCommissions.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserStripeIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Invoice",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 2, 6, 15, 13, 6, 415, DateTimeKind.Local).AddTicks(9090),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 2, 5, 11, 16, 50, 287, DateTimeKind.Local).AddTicks(202));

            migrationBuilder.AddColumn<string>(
                name: "StripeId",
                table: "AppUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripeId",
                table: "AppUsers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Invoice",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 2, 5, 11, 16, 50, 287, DateTimeKind.Local).AddTicks(202),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 2, 6, 15, 13, 6, 415, DateTimeKind.Local).AddTicks(9090));
        }
    }
}
