using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotnetServer.Migrations
{
    /// <inheritdoc />
    public partial class Migration_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Rooms_RoomHash",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistVideos_Rooms_RoomHash",
                table: "PlaylistVideos");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Rooms_RoomHash",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Hash",
                table: "Rooms",
                column: "Hash",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Rooms_RoomHash",
                table: "ChatMessages",
                column: "RoomHash",
                principalTable: "Rooms",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistVideos_Rooms_RoomHash",
                table: "PlaylistVideos",
                column: "RoomHash",
                principalTable: "Rooms",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Rooms_RoomHash",
                table: "Users",
                column: "RoomHash",
                principalTable: "Rooms",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Rooms_RoomHash",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistVideos_Rooms_RoomHash",
                table: "PlaylistVideos");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Rooms_RoomHash",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_Hash",
                table: "Rooms");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Rooms_RoomHash",
                table: "ChatMessages",
                column: "RoomHash",
                principalTable: "Rooms",
                principalColumn: "Hash");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistVideos_Rooms_RoomHash",
                table: "PlaylistVideos",
                column: "RoomHash",
                principalTable: "Rooms",
                principalColumn: "Hash");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Rooms_RoomHash",
                table: "Users",
                column: "RoomHash",
                principalTable: "Rooms",
                principalColumn: "Hash");
        }
    }
}
