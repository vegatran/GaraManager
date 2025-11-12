using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _20251111041000_AddWarehouseAndSku : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "Parts",
                newName: "DefaultUnit");

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "Parts",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "Parts",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "WarehouseBinId",
                table: "PartInventoryBatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "PartInventoryBatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseZoneId",
                table: "PartInventoryBatches",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PartUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PartId = table.Column<int>(type: "int", nullable: false),
                    UnitName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConversionRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Barcode = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_PartUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartUnits_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ManagerName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WarehouseZones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_WarehouseZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseZones_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WarehouseBins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    WarehouseZoneId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Capacity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_WarehouseBins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseBins_WarehouseZones_WarehouseZoneId",
                        column: x => x.WarehouseZoneId,
                        principalTable: "WarehouseZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WarehouseBins_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(2787));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(2790));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(2791));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(2793));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(2794));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(2897));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(3167), new DateTime(2023, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(3105) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(3184), new DateTime(2024, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(3176) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(3016));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(3022));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(3023));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(3025));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(3026));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(3027));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(3029));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(3030));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(3031));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(2521));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(2531));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(2533));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 13, 23, 31, 368, DateTimeKind.Local).AddTicks(2593));

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Barcode",
                table: "Parts",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Sku",
                table: "Parts",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartInventoryBatches_WarehouseBinId",
                table: "PartInventoryBatches",
                column: "WarehouseBinId");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventoryBatches_WarehouseId",
                table: "PartInventoryBatches",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventoryBatches_WarehouseZoneId",
                table: "PartInventoryBatches",
                column: "WarehouseZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_PartUnits_Barcode",
                table: "PartUnits",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartUnits_PartId_UnitName",
                table: "PartUnits",
                columns: new[] { "PartId", "UnitName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseBins_WarehouseId_Code",
                table: "WarehouseBins",
                columns: new[] { "WarehouseId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseBins_WarehouseZoneId",
                table: "WarehouseBins",
                column: "WarehouseZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_Code",
                table: "Warehouses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseZones_WarehouseId_Code",
                table: "WarehouseZones",
                columns: new[] { "WarehouseId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PartInventoryBatches_WarehouseBins_WarehouseBinId",
                table: "PartInventoryBatches",
                column: "WarehouseBinId",
                principalTable: "WarehouseBins",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PartInventoryBatches_WarehouseZones_WarehouseZoneId",
                table: "PartInventoryBatches",
                column: "WarehouseZoneId",
                principalTable: "WarehouseZones",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PartInventoryBatches_Warehouses_WarehouseId",
                table: "PartInventoryBatches",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartInventoryBatches_WarehouseBins_WarehouseBinId",
                table: "PartInventoryBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_PartInventoryBatches_WarehouseZones_WarehouseZoneId",
                table: "PartInventoryBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_PartInventoryBatches_Warehouses_WarehouseId",
                table: "PartInventoryBatches");

            migrationBuilder.DropTable(
                name: "PartUnits");

            migrationBuilder.DropTable(
                name: "WarehouseBins");

            migrationBuilder.DropTable(
                name: "WarehouseZones");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Parts_Barcode",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_Sku",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_PartInventoryBatches_WarehouseBinId",
                table: "PartInventoryBatches");

            migrationBuilder.DropIndex(
                name: "IX_PartInventoryBatches_WarehouseId",
                table: "PartInventoryBatches");

            migrationBuilder.DropIndex(
                name: "IX_PartInventoryBatches_WarehouseZoneId",
                table: "PartInventoryBatches");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "WarehouseBinId",
                table: "PartInventoryBatches");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "PartInventoryBatches");

            migrationBuilder.DropColumn(
                name: "WarehouseZoneId",
                table: "PartInventoryBatches");

            migrationBuilder.RenameColumn(
                name: "DefaultUnit",
                table: "Parts",
                newName: "Unit");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8625));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8627));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8628));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8630));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8631));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8663));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8772), new DateTime(2023, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8755) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8802), new DateTime(2024, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8800) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8702));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8704));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8706));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8707));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8709));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8711));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8712));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8714));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8715));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8569));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8572));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8574));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 11, 10, 29, 44, 509, DateTimeKind.Local).AddTicks(8576));
        }
    }
}
