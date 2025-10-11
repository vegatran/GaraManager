using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlternativePhoneAndContactPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlternativePhone",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPersonName",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 7, 13, 46, 57, 755, DateTimeKind.Local).AddTicks(7711), new DateTime(2023, 10, 7, 13, 46, 57, 755, DateTimeKind.Local).AddTicks(7696) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 7, 13, 46, 57, 755, DateTimeKind.Local).AddTicks(7715), new DateTime(2024, 10, 7, 13, 46, 57, 755, DateTimeKind.Local).AddTicks(7714) });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 7, 13, 46, 57, 755, DateTimeKind.Local).AddTicks(7388));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 7, 13, 46, 57, 755, DateTimeKind.Local).AddTicks(7391));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 7, 13, 46, 57, 755, DateTimeKind.Local).AddTicks(7393));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 7, 13, 46, 57, 755, DateTimeKind.Local).AddTicks(7395));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlternativePhone",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ContactPersonName",
                table: "Customers");

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 7, 13, 36, 0, 890, DateTimeKind.Local).AddTicks(1716), new DateTime(2023, 10, 7, 13, 36, 0, 890, DateTimeKind.Local).AddTicks(1676) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 7, 13, 36, 0, 890, DateTimeKind.Local).AddTicks(1726), new DateTime(2024, 10, 7, 13, 36, 0, 890, DateTimeKind.Local).AddTicks(1719) });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 7, 13, 36, 0, 890, DateTimeKind.Local).AddTicks(838));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 7, 13, 36, 0, 890, DateTimeKind.Local).AddTicks(847));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 7, 13, 36, 0, 890, DateTimeKind.Local).AddTicks(850));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 7, 13, 36, 0, 890, DateTimeKind.Local).AddTicks(852));
        }
    }
}
