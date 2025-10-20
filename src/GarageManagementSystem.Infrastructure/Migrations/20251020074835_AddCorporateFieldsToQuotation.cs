using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCorporateFieldsToQuotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ContractDate",
                table: "ServiceQuotations",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractFilePath",
                table: "ServiceQuotations",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ContractNumber",
                table: "ServiceQuotations",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CorporateApprovalDate",
                table: "ServiceQuotations",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorporateApprovalNotes",
                table: "ServiceQuotations",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "CorporateApprovedAmount",
                table: "ServiceQuotations",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountRate",
                table: "ServiceQuotations",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorporateApprovalNotes",
                table: "QuotationItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "CorporateApprovedSubTotal",
                table: "QuotationItems",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CorporateApprovedTotalAmount",
                table: "QuotationItems",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CorporateApprovedUnitPrice",
                table: "QuotationItems",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CorporateApprovedVATAmount",
                table: "QuotationItems",
                type: "decimal(65,30)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractDate",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "ContractFilePath",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "ContractNumber",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "CorporateApprovalDate",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "CorporateApprovalNotes",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "CorporateApprovedAmount",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "DiscountRate",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "CorporateApprovalNotes",
                table: "QuotationItems");

            migrationBuilder.DropColumn(
                name: "CorporateApprovedSubTotal",
                table: "QuotationItems");

            migrationBuilder.DropColumn(
                name: "CorporateApprovedTotalAmount",
                table: "QuotationItems");

            migrationBuilder.DropColumn(
                name: "CorporateApprovedUnitPrice",
                table: "QuotationItems");

            migrationBuilder.DropColumn(
                name: "CorporateApprovedVATAmount",
                table: "QuotationItems");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1369));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1373));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1377));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1380));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1384));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1387));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(2031), new DateTime(2023, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(2004) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(2042), new DateTime(2024, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(2037) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1461));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1465));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1468));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1472));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1486));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1521));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1524));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1527));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1530));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(843));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(849));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(854));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(859));
        }
    }
}
