using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VocabLab.Migrations
{
    /// <inheritdoc />
    public partial class AddPronunciationToWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Pronounciation",
                table: "Words",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Words",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Pronounciation" },
                values: new object[] { new DateTime(2026, 5, 24, 21, 27, 22, 949, DateTimeKind.Local).AddTicks(9342), "/ˈæpl/" });

            migrationBuilder.UpdateData(
                table: "Words",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Pronounciation" },
                values: new object[] { new DateTime(2026, 5, 24, 21, 27, 22, 949, DateTimeKind.Local).AddTicks(9377), "/dɔːɡ/" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pronounciation",
                table: "Words");

            migrationBuilder.UpdateData(
                table: "Words",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 20, 23, 43, 53, 480, DateTimeKind.Local).AddTicks(7713));

            migrationBuilder.UpdateData(
                table: "Words",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 20, 23, 43, 53, 480, DateTimeKind.Local).AddTicks(7726));
        }
    }
}
