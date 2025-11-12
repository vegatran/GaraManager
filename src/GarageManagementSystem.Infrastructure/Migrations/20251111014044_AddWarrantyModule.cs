using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWarrantyModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WarrantyCode",
                table: "ServiceOrders",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyExpiryDate",
                table: "ServiceOrders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Warranties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WarrantyCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ServiceOrderId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    WarrantyStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    WarrantyEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HandoverBy = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HandoverLocation = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PK_Warranties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Warranties_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Warranties_ServiceOrders_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "ServiceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Warranties_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WarrantyClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClaimNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WarrantyId = table.Column<int>(type: "int", nullable: false),
                    ServiceOrderId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    VehicleId = table.Column<int>(type: "int", nullable: true),
                    ClaimDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IssueDescription = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Resolution = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResolvedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PK_WarrantyClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantyClaims_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarrantyClaims_ServiceOrders_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "ServiceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarrantyClaims_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarrantyClaims_Warranties_WarrantyId",
                        column: x => x.WarrantyId,
                        principalTable: "Warranties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WarrantyItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WarrantyId = table.Column<int>(type: "int", nullable: false),
                    ServiceOrderPartId = table.Column<int>(type: "int", nullable: true),
                    PartId = table.Column<int>(type: "int", nullable: true),
                    PartName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PartNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SerialNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WarrantyMonths = table.Column<int>(type: "int", nullable: false),
                    WarrantyStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    WarrantyEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PK_WarrantyItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantyItems_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarrantyItems_ServiceOrderParts_ServiceOrderPartId",
                        column: x => x.ServiceOrderPartId,
                        principalTable: "ServiceOrderParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WarrantyItems_Warranties_WarrantyId",
                        column: x => x.WarrantyId,
                        principalTable: "Warranties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2388));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2390));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2392));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2393));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2395));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2397));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2669), new DateTime(2023, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2652) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2675), new DateTime(2024, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2672) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2435));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2437));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2439));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2440));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2457));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2470));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2472));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2474));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2475));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2157));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2161));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2163));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 8, 40, 43, 683, DateTimeKind.Local).AddTicks(2166));

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_CustomerId",
                table: "Warranties",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_ServiceOrderId",
                table: "Warranties",
                column: "ServiceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_VehicleId",
                table: "Warranties",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_WarrantyCode",
                table: "Warranties",
                column: "WarrantyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_ClaimNumber",
                table: "WarrantyClaims",
                column: "ClaimNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_CustomerId",
                table: "WarrantyClaims",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_ServiceOrderId",
                table: "WarrantyClaims",
                column: "ServiceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_VehicleId",
                table: "WarrantyClaims",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_WarrantyId",
                table: "WarrantyClaims",
                column: "WarrantyId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyItems_PartId",
                table: "WarrantyItems",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyItems_ServiceOrderPartId",
                table: "WarrantyItems",
                column: "ServiceOrderPartId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyItems_WarrantyId",
                table: "WarrantyItems",
                column: "WarrantyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarrantyClaims");

            migrationBuilder.DropTable(
                name: "WarrantyItems");

            migrationBuilder.DropTable(
                name: "Warranties");

            migrationBuilder.DropColumn(
                name: "WarrantyCode",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "WarrantyExpiryDate",
                table: "ServiceOrders");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2076));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2077));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2079));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2080));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2082));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2083));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2202), new DateTime(2023, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2182) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2207), new DateTime(2024, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2205) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2114));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2116));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2118));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2119));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2131));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2144));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2145));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2146));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(2147));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(1859));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(1861));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(1864));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 10, 16, 56, 58, 801, DateTimeKind.Local).AddTicks(1865));
        }
    }
}
