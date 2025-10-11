using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUniqueIndexesForSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_VIN",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_VehicleInspections_InspectionNumber",
                table: "VehicleInspections");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_SupplierCode",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_StockTransactions_TransactionNumber",
                table: "StockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ServiceQuotations_QuotationNumber",
                table: "ServiceQuotations");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrders_OrderNumber",
                table: "ServiceOrders");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_ReceiptNumber",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Parts_PartNumber",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Email",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Phone",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_AppointmentNumber",
                table: "Appointments");

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

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles",
                column: "LicensePlate",
                unique: true,
                filter: "[LicensePlate] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_VIN",
                table: "Vehicles",
                column: "VIN",
                unique: true,
                filter: "[VIN] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspections_InspectionNumber",
                table: "VehicleInspections",
                column: "InspectionNumber",
                unique: true,
                filter: "[InspectionNumber] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_SupplierCode",
                table: "Suppliers",
                column: "SupplierCode",
                unique: true,
                filter: "[SupplierCode] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_TransactionNumber",
                table: "StockTransactions",
                column: "TransactionNumber",
                unique: true,
                filter: "[TransactionNumber] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceQuotations_QuotationNumber",
                table: "ServiceQuotations",
                column: "QuotationNumber",
                unique: true,
                filter: "[QuotationNumber] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_OrderNumber",
                table: "ServiceOrders",
                column: "OrderNumber",
                unique: true,
                filter: "[OrderNumber] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ReceiptNumber",
                table: "PaymentTransactions",
                column: "ReceiptNumber",
                unique: true,
                filter: "[ReceiptNumber] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartNumber",
                table: "Parts",
                column: "PartNumber",
                unique: true,
                filter: "[PartNumber] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Phone",
                table: "Customers",
                column: "Phone",
                unique: true,
                filter: "[Phone] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentNumber",
                table: "Appointments",
                column: "AppointmentNumber",
                unique: true,
                filter: "[AppointmentNumber] IS NOT NULL AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_VIN",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_VehicleInspections_InspectionNumber",
                table: "VehicleInspections");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_SupplierCode",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_StockTransactions_TransactionNumber",
                table: "StockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ServiceQuotations_QuotationNumber",
                table: "ServiceQuotations");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrders_OrderNumber",
                table: "ServiceOrders");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_ReceiptNumber",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Parts_PartNumber",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Email",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Phone",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_AppointmentNumber",
                table: "Appointments");

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

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_VIN",
                table: "Vehicles",
                column: "VIN",
                unique: true,
                filter: "[VIN] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspections_InspectionNumber",
                table: "VehicleInspections",
                column: "InspectionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_SupplierCode",
                table: "Suppliers",
                column: "SupplierCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_TransactionNumber",
                table: "StockTransactions",
                column: "TransactionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceQuotations_QuotationNumber",
                table: "ServiceQuotations",
                column: "QuotationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_OrderNumber",
                table: "ServiceOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ReceiptNumber",
                table: "PaymentTransactions",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartNumber",
                table: "Parts",
                column: "PartNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Phone",
                table: "Customers",
                column: "Phone",
                unique: true,
                filter: "[Phone] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentNumber",
                table: "Appointments",
                column: "AppointmentNumber",
                unique: true);
        }
    }
}
