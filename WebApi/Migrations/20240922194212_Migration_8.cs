using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class Migration_8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoPlayers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VideoPlayers",
                columns: table => new
                {
                    RoomHash = table.Column<string>(type: "text", nullable: false),
                    PlaylistVideoHash = table.Column<string>(type: "text", nullable: true),
                    SetPlayedSecondsCalled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoPlayers", x => x.RoomHash);
                    table.ForeignKey(
                        name: "FK_VideoPlayers_PlaylistVideos_PlaylistVideoHash",
                        column: x => x.PlaylistVideoHash,
                        principalTable: "PlaylistVideos",
                        principalColumn: "Hash");
                    table.ForeignKey(
                        name: "FK_VideoPlayers_Rooms_RoomHash",
                        column: x => x.RoomHash,
                        principalTable: "Rooms",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoPlayers_PlaylistVideoHash",
                table: "VideoPlayers",
                column: "PlaylistVideoHash");
        }
    }
}
