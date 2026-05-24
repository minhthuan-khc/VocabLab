using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VocabLab.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Pronounciation",
                table: "Words",
                newName: "Pronunciation");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Pronunciation",
                table: "Words",
                newName: "Pronounciation");

            migrationBuilder.UpdateData(
                table: "Words",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 24, 21, 27, 22, 949, DateTimeKind.Local).AddTicks(9342));

            migrationBuilder.UpdateData(
                table: "Words",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 24, 21, 27, 22, 949, DateTimeKind.Local).AddTicks(9377));
        }
    }
}
