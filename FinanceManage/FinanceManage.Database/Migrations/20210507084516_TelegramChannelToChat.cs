using Microsoft.EntityFrameworkCore.Migrations;

namespace FinanceManage.Database.Migrations
{
    public partial class TelegramChannelToChat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TelegramChannelId",
                table: "Purchases",
                newName: "TelegramChatId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TelegramChatId",
                table: "Purchases",
                newName: "TelegramChannelId");
        }
    }
}
