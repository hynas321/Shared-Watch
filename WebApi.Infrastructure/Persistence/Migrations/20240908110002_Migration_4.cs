using System;
using WebApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Migration_4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"ChatMessages\" ALTER COLUMN \"Date\" TYPE timestamp with time zone USING \"Date\"::timestamp with time zone");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"ChatMessages\" ALTER COLUMN \"Date\" TYPE text");
        }
    }
}
