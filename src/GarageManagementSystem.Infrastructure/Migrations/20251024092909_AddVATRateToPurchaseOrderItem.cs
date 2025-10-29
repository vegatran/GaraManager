using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVATRateToPurchaseOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "VATAmount",
                table: "PurchaseOrderItems",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VATRate",
                table: "PurchaseOrderItems",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 10m);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3883));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3885));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3886));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3888));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3890));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3891));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(4091), new DateTime(2023, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(4069) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(4098), new DateTime(2024, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(4094) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3933));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3935));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3937));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3938));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3947));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3960));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3961));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3963));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3964));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3417));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3422));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3464));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 16, 29, 4, 637, DateTimeKind.Local).AddTicks(3467));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VATAmount",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "VATRate",
                table: "PurchaseOrderItems");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2463));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2465));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2467));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2469));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2471));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2472));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2626), new DateTime(2023, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2613) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2631), new DateTime(2024, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2629) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2506));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2508));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2510));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2512));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2551));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2570));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2572));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2573));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2575));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2223));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2235));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2238));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 16, 43, 27, 158, DateTimeKind.Local).AddTicks(2240));
        }
    }
}
