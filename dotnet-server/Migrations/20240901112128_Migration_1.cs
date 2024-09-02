using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_server.Migrations
{
    /// <inheritdoc />
    public partial class Migration_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Hash = table.Column<string>(type: "text", nullable: false),
                    AdminTokens = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Hash);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Username = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    RoomHash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => new { x.Username, x.Date });
                    table.ForeignKey(
                        name: "FK_ChatMessages_Rooms_RoomHash",
                        column: x => x.RoomHash,
                        principalTable: "Rooms",
                        principalColumn: "Hash");
                });

            migrationBuilder.CreateTable(
                name: "PlaylistVideos",
                columns: table => new
                {
                    Hash = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Duration = table.Column<double>(type: "double precision", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: true),
                    RoomHash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistVideos", x => x.Hash);
                    table.ForeignKey(
                        name: "FK_PlaylistVideos_Rooms_RoomHash",
                        column: x => x.RoomHash,
                        principalTable: "Rooms",
                        principalColumn: "Hash");
                });

            migrationBuilder.CreateTable(
                name: "RoomSettings",
                columns: table => new
                {
                    RoomHash = table.Column<string>(type: "text", nullable: false),
                    RoomName = table.Column<string>(type: "text", nullable: false),
                    RoomPassword = table.Column<string>(type: "text", nullable: true),
                    RoomType = table.Column<int>(type: "integer", nullable: false),
                    MaxUsers = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomSettings", x => x.RoomHash);
                    table.ForeignKey(
                        name: "FK_RoomSettings_Rooms_RoomHash",
                        column: x => x.RoomHash,
                        principalTable: "Rooms",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    RoomHash = table.Column<string>(type: "text", nullable: false),
                    CanAddChatMessage = table.Column<bool>(type: "boolean", nullable: false),
                    CanAddVideo = table.Column<bool>(type: "boolean", nullable: false),
                    CanRemoveVideo = table.Column<bool>(type: "boolean", nullable: false),
                    CanStartOrPauseVideo = table.Column<bool>(type: "boolean", nullable: false),
                    CanSkipVideo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => x.RoomHash);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Rooms_RoomHash",
                        column: x => x.RoomHash,
                        principalTable: "Rooms",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    AuthorizationToken = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    SignalRConnectionId = table.Column<string>(type: "text", nullable: true),
                    RoomHash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.AuthorizationToken);
                    table.ForeignKey(
                        name: "FK_Users_Rooms_RoomHash",
                        column: x => x.RoomHash,
                        principalTable: "Rooms",
                        principalColumn: "Hash");
                });

            migrationBuilder.CreateTable(
                name: "VideoPlayers",
                columns: table => new
                {
                    RoomHash = table.Column<string>(type: "text", nullable: false),
                    PlaylistVideoHash = table.Column<string>(type: "text", nullable: true),
                    IsPlaying = table.Column<bool>(type: "boolean", nullable: false),
                    CurrentTime = table.Column<double>(type: "double precision", nullable: false),
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
                name: "IX_ChatMessages_RoomHash",
                table: "ChatMessages",
                column: "RoomHash");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistVideos_RoomHash",
                table: "PlaylistVideos",
                column: "RoomHash");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoomHash",
                table: "Users",
                column: "RoomHash");

            migrationBuilder.CreateIndex(
                name: "IX_VideoPlayers_PlaylistVideoHash",
                table: "VideoPlayers",
                column: "PlaylistVideoHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "RoomSettings");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "VideoPlayers");

            migrationBuilder.DropTable(
                name: "PlaylistVideos");

            migrationBuilder.DropTable(
                name: "Rooms");
        }
    }
}
