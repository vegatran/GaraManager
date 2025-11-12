using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceFees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceFeeTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsSystem = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_ServiceFeeTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServiceOrderFees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ServiceOrderId = table.Column<int>(type: "int", nullable: false),
                    ServiceFeeTypeId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VatAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReferenceSource = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsManual = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_ServiceOrderFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceOrderFees_ServiceFeeTypes_ServiceFeeTypeId",
                        column: x => x.ServiceFeeTypeId,
                        principalTable: "ServiceFeeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceOrderFees_ServiceOrders_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "ServiceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(4924));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(4925));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(4927));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(4928));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(4930));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(4931));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(5123), new DateTime(2023, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(5105) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(5128), new DateTime(2024, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(5126) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(4971));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(4973));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(5059));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(5061));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(5062));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(5064));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(5065));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(5066));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(5068));

            migrationBuilder.InsertData(
                table: "ServiceFeeTypes",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsActive", "IsDeleted", "IsSystem", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "LABOR", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), null, null, null, "Tiền công kỹ thuật viên", 1, true, false, true, "Công sửa chữa", null, null },
                    { 2, "PARTS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), null, null, null, "Chi phí phụ tùng, vật tư", 2, true, false, true, "Phụ tùng", null, null },
                    { 3, "CONSUMABLE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), null, null, null, "Dầu nhớt, dung dịch, vật tư tiêu hao", 3, true, false, true, "Vật tư tiêu hao", null, null },
                    { 4, "SUBLET", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), null, null, null, "Thuê ngoài/đối tác thực hiện", 4, true, false, true, "Dịch vụ ngoài", null, null },
                    { 5, "OTHER", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), null, null, null, "Các khoản phí khác", 5, true, false, true, "Phí khác", null, null }
                });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(4872));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(4874));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(4877));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 9, 47, 11, 963, DateTimeKind.Local).AddTicks(4879));

            migrationBuilder.CreateIndex(
                name: "IX_ServiceFeeTypes_Code",
                table: "ServiceFeeTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrderFees_ServiceFeeTypeId",
                table: "ServiceOrderFees",
                column: "ServiceFeeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrderFees_ServiceOrderId",
                table: "ServiceOrderFees",
                column: "ServiceOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceOrderFees");

            migrationBuilder.DropTable(
                name: "ServiceFeeTypes");

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
        }
    }
}
