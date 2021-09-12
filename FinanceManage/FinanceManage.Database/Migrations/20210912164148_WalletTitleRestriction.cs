using Microsoft.EntityFrameworkCore.Migrations;

namespace FinanceManage.Database.Migrations
{
    public partial class WalletTitleRestriction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WalletType",
                table: "Wallets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_Title_TelegramChatId",
                table: "Wallets",
                columns: new[] { "Title", "TelegramChatId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wallets_Title_TelegramChatId",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "WalletType",
                table: "Wallets");
        }
    }
}
