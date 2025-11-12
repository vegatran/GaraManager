using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianAssignmentToServiceOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedHours",
                table: "ServiceOrderLabors",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "AssignedTechnicianId",
                table: "ServiceOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedHours",
                table: "ServiceOrderItems",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(73));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(75));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(76));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(78));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(79));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(80));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(334), new DateTime(2023, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(305) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(339), new DateTime(2024, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(337) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(136));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(138));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(140));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(141));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(154));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(170));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(172));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(173));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 95, DateTimeKind.Local).AddTicks(175));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 94, DateTimeKind.Local).AddTicks(9562));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 94, DateTimeKind.Local).AddTicks(9578));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 94, DateTimeKind.Local).AddTicks(9581));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 17, 11, 24, 94, DateTimeKind.Local).AddTicks(9583));

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrderItems_AssignedTechnicianId",
                table: "ServiceOrderItems",
                column: "AssignedTechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrderItems_Employees_AssignedTechnicianId",
                table: "ServiceOrderItems",
                column: "AssignedTechnicianId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrderItems_Employees_AssignedTechnicianId",
                table: "ServiceOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrderItems_AssignedTechnicianId",
                table: "ServiceOrderItems");

            migrationBuilder.DropColumn(
                name: "EstimatedHours",
                table: "ServiceOrderLabors");

            migrationBuilder.DropColumn(
                name: "AssignedTechnicianId",
                table: "ServiceOrderItems");

            migrationBuilder.DropColumn(
                name: "EstimatedHours",
                table: "ServiceOrderItems");

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
    }
}
