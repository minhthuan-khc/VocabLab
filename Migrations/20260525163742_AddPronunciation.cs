using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VocabLab.Migrations
{
    /// <inheritdoc />
    public partial class AddPronunciation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Words",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 25, 23, 37, 39, 623, DateTimeKind.Local).AddTicks(3897));

            migrationBuilder.UpdateData(
                table: "Words",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 25, 23, 37, 39, 623, DateTimeKind.Local).AddTicks(3917));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Words",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 24, 22, 4, 47, 722, DateTimeKind.Local).AddTicks(1665));

            migrationBuilder.UpdateData(
                table: "Words",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 24, 22, 4, 47, 722, DateTimeKind.Local).AddTicks(1684));
        }
    }
}
