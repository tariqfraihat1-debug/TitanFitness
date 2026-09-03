using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TitanFitness.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedBranchesAndStudios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "BranchId", "ClosingTime", "Name", "OpeningTime" },
                values: new object[,]
                {
                    { 1, new TimeOnly(23, 0, 0), "Amman Main Branch", new TimeOnly(6, 0, 0) },
                    { 2, new TimeOnly(23, 0, 0), "Khalda Branch", new TimeOnly(6, 0, 0) },
                    { 3, new TimeOnly(22, 0, 0), "Abdoun Branch", new TimeOnly(7, 0, 0) },
                    { 4, new TimeOnly(22, 30, 0), "Sweifieh Branch", new TimeOnly(6, 30, 0) }
                });

            migrationBuilder.InsertData(
                table: "Studios",
                columns: new[] { "StudioId", "BranchId", "Capacity", "Name" },
                values: new object[,]
                {
                    { 1, 1, 20, "Studio A" },
                    { 2, 1, 15, "Zen Room" },
                    { 3, 1, 25, "Cycle Studio" },
                    { 4, 2, 20, "Studio A" },
                    { 5, 2, 18, "Strength Studio" },
                    { 6, 2, 25, "Cycle Studio" },
                    { 7, 3, 15, "Yoga Studio" },
                    { 8, 3, 20, "Studio B" },
                    { 9, 3, 16, "Functional Studio" },
                    { 10, 4, 20, "Studio A" },
                    { 11, 4, 15, "Zen Room" },
                    { 12, 4, 25, "Cycle Studio" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Studios",
                keyColumn: "StudioId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Studios",
                keyColumn: "StudioId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Studios",
                keyColumn: "StudioId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Studios",
                keyColumn: "StudioId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Studios",
                keyColumn: "StudioId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Studios",
                keyColumn: "StudioId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Studios",
                keyColumn: "StudioId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Studios",
                keyColumn: "StudioId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Studios",
                keyColumn: "StudioId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Studios",
                keyColumn: "StudioId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Studios",
                keyColumn: "StudioId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Studios",
                keyColumn: "StudioId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "BranchId",
                keyValue: 4);
        }
    }
}
