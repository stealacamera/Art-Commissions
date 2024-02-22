using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtCommissions.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemovedProfilePictureColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePicturePath",
                table: "AppUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePicturePath",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
