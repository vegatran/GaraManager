using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleTypeAndApprovalWorkflows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdjusterName",
                table: "Vehicles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdjusterPhone",
                table: "Vehicles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClaimNumber",
                table: "Vehicles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Vehicles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "Vehicles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Vehicles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CostCenter",
                table: "Vehicles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverageType",
                table: "Vehicles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Vehicles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceCompany",
                table: "Vehicles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PolicyNumber",
                table: "Vehicles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxCode",
                table: "Vehicles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleType",
                table: "Vehicles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Personal");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompanyApprovalDate",
                table: "ServiceQuotations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyApprovalNotes",
                table: "ServiceQuotations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyApprovedBy",
                table: "ServiceQuotations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyContactPerson",
                table: "ServiceQuotations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Deductible",
                table: "ServiceQuotations",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceAdjusterContact",
                table: "ServiceQuotations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InsuranceApprovalDate",
                table: "ServiceQuotations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceApprovalNotes",
                table: "ServiceQuotations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceApprovedAmount",
                table: "ServiceQuotations",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTaxExempt",
                table: "ServiceQuotations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxInsuranceAmount",
                table: "ServiceQuotations",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PONumber",
                table: "ServiceQuotations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTerms",
                table: "ServiceQuotations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "Cash");

            migrationBuilder.AddColumn<string>(
                name: "QuotationType",
                table: "ServiceQuotations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Personal");

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 29, 21, 734, DateTimeKind.Local).AddTicks(6982), new DateTime(2023, 10, 7, 8, 29, 21, 734, DateTimeKind.Local).AddTicks(6967) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 29, 21, 734, DateTimeKind.Local).AddTicks(6987), new DateTime(2024, 10, 7, 8, 29, 21, 734, DateTimeKind.Local).AddTicks(6985) });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 7, 8, 29, 21, 734, DateTimeKind.Local).AddTicks(6667));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 7, 8, 29, 21, 734, DateTimeKind.Local).AddTicks(6669));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 7, 8, 29, 21, 734, DateTimeKind.Local).AddTicks(6672));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 7, 8, 29, 21, 734, DateTimeKind.Local).AddTicks(6674));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdjusterName",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "AdjusterPhone",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ClaimNumber",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "CostCenter",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "CoverageType",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "InsuranceCompany",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "PolicyNumber",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TaxCode",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "CompanyApprovalDate",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "CompanyApprovalNotes",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "CompanyApprovedBy",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "CompanyContactPerson",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "Deductible",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "InsuranceAdjusterContact",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "InsuranceApprovalDate",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "InsuranceApprovalNotes",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "InsuranceApprovedAmount",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "IsTaxExempt",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "MaxInsuranceAmount",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "PONumber",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "PaymentTerms",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "QuotationType",
                table: "ServiceQuotations");

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 6, 16, 58, 41, 179, DateTimeKind.Local).AddTicks(7872), new DateTime(2023, 10, 6, 16, 58, 41, 179, DateTimeKind.Local).AddTicks(7858) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 6, 16, 58, 41, 179, DateTimeKind.Local).AddTicks(7877), new DateTime(2024, 10, 6, 16, 58, 41, 179, DateTimeKind.Local).AddTicks(7875) });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 6, 16, 58, 41, 179, DateTimeKind.Local).AddTicks(7577));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 6, 16, 58, 41, 179, DateTimeKind.Local).AddTicks(7580));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 6, 16, 58, 41, 179, DateTimeKind.Local).AddTicks(7582));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 6, 16, 58, 41, 179, DateTimeKind.Local).AddTicks(7584));
        }
    }
}
