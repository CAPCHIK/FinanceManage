using Microsoft.EntityFrameworkCore.Migrations;

namespace FinanceManage.Database.Migrations
{
    public partial class SplitCategoryWithDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Purchases",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Purchases");
        }
    }
}
