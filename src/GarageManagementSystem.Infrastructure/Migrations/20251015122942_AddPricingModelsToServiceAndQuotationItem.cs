using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingModelsToServiceAndQuotationItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVATApplicable",
                table: "Services",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialCost",
                table: "Services",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PricingModel",
                table: "Services",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PricingNotes",
                table: "Services",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "VATRate",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsVATApplicable",
                table: "QuotationItems",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LaborCost",
                table: "QuotationItems",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialCost",
                table: "QuotationItems",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PricingBreakdown",
                table: "QuotationItems",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PricingModel",
                table: "QuotationItems",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5481));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5483));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5484));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5486));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5487));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5488));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5584), new DateTime(2023, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5572) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5621), new DateTime(2024, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5619) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5514));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5516));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5517));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5518));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5526));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5535));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5536));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5537));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5539));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "IsVATApplicable", "MaterialCost", "PricingModel", "PricingNotes", "VATRate" },
                values: new object[] { new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5331), true, 0m, "Combined", null, 10 });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "IsVATApplicable", "MaterialCost", "PricingModel", "PricingNotes", "VATRate" },
                values: new object[] { new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5335), true, 0m, "Combined", null, 10 });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "IsVATApplicable", "MaterialCost", "PricingModel", "PricingNotes", "VATRate" },
                values: new object[] { new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5337), true, 0m, "Combined", null, 10 });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "IsVATApplicable", "MaterialCost", "PricingModel", "PricingNotes", "VATRate" },
                values: new object[] { new DateTime(2025, 10, 15, 19, 29, 40, 830, DateTimeKind.Local).AddTicks(5339), true, 0m, "Combined", null, 10 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVATApplicable",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "MaterialCost",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "PricingModel",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "PricingNotes",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "VATRate",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsVATApplicable",
                table: "QuotationItems");

            migrationBuilder.DropColumn(
                name: "LaborCost",
                table: "QuotationItems");

            migrationBuilder.DropColumn(
                name: "MaterialCost",
                table: "QuotationItems");

            migrationBuilder.DropColumn(
                name: "PricingBreakdown",
                table: "QuotationItems");

            migrationBuilder.DropColumn(
                name: "PricingModel",
                table: "QuotationItems");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7813));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7815));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7816));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7818));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7819));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7821));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7979), new DateTime(2023, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7959) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7990), new DateTime(2024, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7988) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7865));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7867));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7868));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7869));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7882));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7904));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7905));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7907));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7908));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7482));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7489));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7492));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7494));
        }
    }
}
