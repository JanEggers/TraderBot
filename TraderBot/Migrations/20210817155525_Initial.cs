using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TraderBot.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Symbols",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Symbols", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeSeries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SymbolId = table.Column<int>(type: "INTEGER", nullable: false),
                    Interval = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSeries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeSeries_Symbols_SymbolId",
                        column: x => x.SymbolId,
                        principalTable: "Symbols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockDataPoints",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimeSeriesId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClosingPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    OpeningPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    LowestPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    HighestPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Volume = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockDataPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockDataPoints_TimeSeries_TimeSeriesId",
                        column: x => x.TimeSeriesId,
                        principalTable: "TimeSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockDataPoints_TimeSeriesId_Time",
                table: "StockDataPoints",
                columns: new[] { "TimeSeriesId", "Time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Symbols_Name",
                table: "Symbols",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeSeries_SymbolId_Interval",
                table: "TimeSeries",
                columns: new[] { "SymbolId", "Interval" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockDataPoints");

            migrationBuilder.DropTable(
                name: "TimeSeries");

            migrationBuilder.DropTable(
                name: "Symbols");
        }
    }
}
