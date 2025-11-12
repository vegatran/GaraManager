using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalQuotationAndOrderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdditionalQuotation",
                table: "ServiceQuotations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RelatedToServiceOrderId",
                table: "ServiceQuotations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdditionalOrder",
                table: "ServiceOrders",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ParentServiceOrderId",
                table: "ServiceOrders",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7852));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7854));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7855));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7856));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7858));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7859));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7975), new DateTime(2023, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7964) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7982), new DateTime(2024, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7977) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7886));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7887));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7889));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7890));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7898));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7912));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7913));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7914));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7916));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7679));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7682));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7684));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 14, 15, 6, 109, DateTimeKind.Local).AddTicks(7686));

            migrationBuilder.CreateIndex(
                name: "IX_ServiceQuotations_RelatedToServiceOrderId",
                table: "ServiceQuotations",
                column: "RelatedToServiceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_ParentServiceOrderId",
                table: "ServiceOrders",
                column: "ParentServiceOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrders_ServiceOrders_ParentServiceOrderId",
                table: "ServiceOrders",
                column: "ParentServiceOrderId",
                principalTable: "ServiceOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceQuotations_ServiceOrders_RelatedToServiceOrderId",
                table: "ServiceQuotations",
                column: "RelatedToServiceOrderId",
                principalTable: "ServiceOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrders_ServiceOrders_ParentServiceOrderId",
                table: "ServiceOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceQuotations_ServiceOrders_RelatedToServiceOrderId",
                table: "ServiceQuotations");

            migrationBuilder.DropIndex(
                name: "IX_ServiceQuotations_RelatedToServiceOrderId",
                table: "ServiceQuotations");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrders_ParentServiceOrderId",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "IsAdditionalQuotation",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "RelatedToServiceOrderId",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "IsAdditionalOrder",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "ParentServiceOrderId",
                table: "ServiceOrders");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(8717));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(8719));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(8720));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(8722));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(8723));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(8725));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(9090), new DateTime(2023, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(9071) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(9116), new DateTime(2024, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(9092) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(8852));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(8855));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(8857));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(8858));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(8889));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(8943));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(8945));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(8946));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(8947));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(7855));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(7867));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(7870));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 14, 40, 33, 886, DateTimeKind.Local).AddTicks(7872));
        }
    }
}
