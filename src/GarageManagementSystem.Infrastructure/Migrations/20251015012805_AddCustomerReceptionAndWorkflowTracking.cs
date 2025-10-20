using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerReceptionAndWorkflowTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerReceptionId",
                table: "VehicleInspections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerReceptionId",
                table: "ServiceQuotations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerReceptionId",
                table: "ServiceOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerReceptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReceptionNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    ReceptionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CustomerRequest = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerComplaints = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReceptionNotes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssignedTechnicianId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssignedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    InspectionStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    InspectionCompletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Priority = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ServiceType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsInsuranceClaim = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    InsuranceCompany = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InsurancePolicyNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmergencyContact = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmergencyContactName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerPhone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VehiclePlate = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VehicleMake = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VehicleModel = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VehicleYear = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerReceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerReceptions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerReceptions_Employees_AssignedTechnicianId",
                        column: x => x.AssignedTechnicianId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CustomerReceptions_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8914));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8915));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8917));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8918));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8919));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8921));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(9032), new DateTime(2023, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(9018) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(9035), new DateTime(2024, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(9034) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8950));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8952));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8954));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8955));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8965));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8976));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8977));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8978));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8979));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8691));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8694));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8763));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 8, 28, 2, 679, DateTimeKind.Local).AddTicks(8765));

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspections_CustomerReceptionId",
                table: "VehicleInspections",
                column: "CustomerReceptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceQuotations_CustomerReceptionId",
                table: "ServiceQuotations",
                column: "CustomerReceptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_CustomerReceptionId",
                table: "ServiceOrders",
                column: "CustomerReceptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReceptions_AssignedTechnicianId",
                table: "CustomerReceptions",
                column: "AssignedTechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReceptions_CustomerId",
                table: "CustomerReceptions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReceptions_ReceptionNumber",
                table: "CustomerReceptions",
                column: "ReceptionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReceptions_VehicleId",
                table: "CustomerReceptions",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrders_CustomerReceptions_CustomerReceptionId",
                table: "ServiceOrders",
                column: "CustomerReceptionId",
                principalTable: "CustomerReceptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceQuotations_CustomerReceptions_CustomerReceptionId",
                table: "ServiceQuotations",
                column: "CustomerReceptionId",
                principalTable: "CustomerReceptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleInspections_CustomerReceptions_CustomerReceptionId",
                table: "VehicleInspections",
                column: "CustomerReceptionId",
                principalTable: "CustomerReceptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrders_CustomerReceptions_CustomerReceptionId",
                table: "ServiceOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceQuotations_CustomerReceptions_CustomerReceptionId",
                table: "ServiceQuotations");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleInspections_CustomerReceptions_CustomerReceptionId",
                table: "VehicleInspections");

            migrationBuilder.DropTable(
                name: "CustomerReceptions");

            migrationBuilder.DropIndex(
                name: "IX_VehicleInspections_CustomerReceptionId",
                table: "VehicleInspections");

            migrationBuilder.DropIndex(
                name: "IX_ServiceQuotations_CustomerReceptionId",
                table: "ServiceQuotations");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrders_CustomerReceptionId",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "CustomerReceptionId",
                table: "VehicleInspections");

            migrationBuilder.DropColumn(
                name: "CustomerReceptionId",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "CustomerReceptionId",
                table: "ServiceOrders");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(8875));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(8886));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(8889));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(8891));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(8897));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(8900));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(9268), new DateTime(2023, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(9225) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(9282), new DateTime(2024, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(9277) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(8998));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(9002));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(9004));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(9006));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(9045));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(9090));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(9092));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(9094));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(7678));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(7684));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(7688));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 17, 34, 13, 420, DateTimeKind.Local).AddTicks(7691));
        }
    }
}
