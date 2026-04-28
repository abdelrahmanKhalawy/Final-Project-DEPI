using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SehhaTech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "MustResetPassword", "PasswordHash", "ProfileImageUrl", "Role", "TenantId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "abdelrahman@sehhatech.com", "Abdelrahman Khalawy", true, false, "$2a$11$Ra0vGMXtltWL94izAo1EP.6ye.tFmO9JUJijBEIzYXzU1n2cBzqHy", null, "SuperAdmin", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "naglaa@sehhatech.com", "Naglaa Shawky", true, false, "$2a$11$Ra0vGMXtltWL94izAo1EP.6ye.tFmO9JUJijBEIzYXzU1n2cBzqHy", null, "SuperAdmin", null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mariam@sehhatech.com", "Mariam Khaled", true, false, "$2a$11$Ra0vGMXtltWL94izAo1EP.6ye.tFmO9JUJijBEIzYXzU1n2cBzqHy", null, "SuperAdmin", null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "shahd@sehhatech.com", "Shahd Abdelaziz", true, false, "$2a$11$Ra0vGMXtltWL94izAo1EP.6ye.tFmO9JUJijBEIzYXzU1n2cBzqHy", null, "SuperAdmin", null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "baher@sehhatech.com", "Baher Khedr", true, false, "$2a$11$Ra0vGMXtltWL94izAo1EP.6ye.tFmO9JUJijBEIzYXzU1n2cBzqHy", null, "SuperAdmin", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
