using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActualHoursToServiceOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ActualHours",
                table: "ServiceOrderItems",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedTime",
                table: "ServiceOrderItems",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "ServiceOrderItems",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "ServiceOrderItems",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(7999));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8001));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8003));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8005));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8007));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8009));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8190), new DateTime(2023, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8174) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8197), new DateTime(2024, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8194) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8067));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8069));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8071));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8073));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8091));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8102));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8104));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8106));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8108));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(7565));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(7569));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(7572));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(7633));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualHours",
                table: "ServiceOrderItems");

            migrationBuilder.DropColumn(
                name: "CompletedTime",
                table: "ServiceOrderItems");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ServiceOrderItems");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "ServiceOrderItems");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7780));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7782));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7783));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7785));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7787));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7788));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(8040), new DateTime(2023, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(8014) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(8046), new DateTime(2024, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(8044) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7850));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7852));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7853));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7906));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7929));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7953));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7955));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7957));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7959));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7412));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7415));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7418));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7421));
        }
    }
}
