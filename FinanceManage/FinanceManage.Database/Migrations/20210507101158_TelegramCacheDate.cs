using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FinanceManage.Database.Migrations
{
    public partial class TelegramCacheDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CachedDate",
                table: "TelegramChatInfoCache",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CachedDate",
                table: "TelegramChatInfoCache");
        }
    }
}
