using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TraderBot.Migrations
{
    /// <inheritdoc />
    public partial class RemovedInterval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimeSeries_SymbolId_Interval",
                table: "TimeSeries");

            migrationBuilder.DropColumn(
                name: "Interval",
                table: "TimeSeries");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSeries_SymbolId",
                table: "TimeSeries",
                column: "SymbolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimeSeries_SymbolId",
                table: "TimeSeries");

            migrationBuilder.AddColumn<string>(
                name: "Interval",
                table: "TimeSeries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSeries_SymbolId_Interval",
                table: "TimeSeries",
                columns: new[] { "SymbolId", "Interval" },
                unique: true);
        }
    }
}
