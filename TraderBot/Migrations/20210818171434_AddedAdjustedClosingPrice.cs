using Microsoft.EntityFrameworkCore.Migrations;

namespace TraderBot.Migrations
{
    public partial class AddedAdjustedClosingPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdjustedClosingPrice",
                table: "StockDataPoints",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdjustedClosingPrice",
                table: "StockDataPoints");
        }
    }
}
