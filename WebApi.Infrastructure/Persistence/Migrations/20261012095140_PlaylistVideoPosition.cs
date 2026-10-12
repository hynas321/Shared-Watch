using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PlaylistVideoPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "PlaylistVideos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                WITH ordered AS (
                    SELECT "Hash", ROW_NUMBER() OVER (PARTITION BY "RoomHash" ORDER BY "Hash") - 1 AS "Position"
                    FROM "PlaylistVideos"
                )
                UPDATE "PlaylistVideos" AS playlist
                SET "Position" = ordered."Position"
                FROM ordered
                WHERE playlist."Hash" = ordered."Hash";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "PlaylistVideos");
        }
    }
}
