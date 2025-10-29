using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVATFieldsToPartAndQuotationItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OverrideIsVATApplicable",
                table: "QuotationItems",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OverrideVATRate",
                table: "QuotationItems",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VATRate",
                table: "PurchaseOrders",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsVATApplicable",
                table: "Parts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "VATRate",
                table: "Parts",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverrideIsVATApplicable",
                table: "QuotationItems");

            migrationBuilder.DropColumn(
                name: "OverrideVATRate",
                table: "QuotationItems");

            migrationBuilder.DropColumn(
                name: "VATRate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "IsVATApplicable",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "VATRate",
                table: "Parts");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5820));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5822));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5824));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5825));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5827));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5828));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5960), new DateTime(2023, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5941) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5963), new DateTime(2024, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5962) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5863));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5865));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5867));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5868));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5879));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5896));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5897));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5899));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5900));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5598));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5632));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5634));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 20, 14, 48, 32, 679, DateTimeKind.Local).AddTicks(5636));
        }
    }
}
