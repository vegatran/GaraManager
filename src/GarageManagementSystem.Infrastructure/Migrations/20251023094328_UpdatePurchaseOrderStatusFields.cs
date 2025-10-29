using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePurchaseOrderStatusFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "PurchaseOrders",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CancelledBy",
                table: "PurchaseOrders",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledDate",
                table: "PurchaseOrders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedDate",
                table: "PurchaseOrders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentDate",
                table: "PurchaseOrders",
                type: "datetime(6)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CancelledDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ReceivedDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "SentDate",
                table: "PurchaseOrders");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(341));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(344));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(347));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(350));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(353));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(355));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(705), new DateTime(2023, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(664) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(722), new DateTime(2024, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(710) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(452));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(457));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(460));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(463));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(511));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(546));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(549));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(551));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 319, DateTimeKind.Local).AddTicks(554));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 318, DateTimeKind.Local).AddTicks(9653));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 318, DateTimeKind.Local).AddTicks(9662));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 318, DateTimeKind.Local).AddTicks(9666));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 17, 28, 7, 318, DateTimeKind.Local).AddTicks(9670));
        }
    }
}
