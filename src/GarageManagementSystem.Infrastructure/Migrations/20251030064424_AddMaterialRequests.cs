using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MRNumber = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ServiceOrderId = table.Column<int>(type: "int", nullable: false),
                    RequestedById = table.Column<int>(type: "int", nullable: false),
                    ApprovedById = table.Column<int>(type: "int", nullable: true),
                    IssuedById = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RejectReason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
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
                    table.PrimaryKey("PK_MaterialRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialRequests_ServiceOrders_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "ServiceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MaterialRequestItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MaterialRequestId = table.Column<int>(type: "int", nullable: false),
                    PartId = table.Column<int>(type: "int", nullable: false),
                    QuantityRequested = table.Column<int>(type: "int", nullable: false),
                    QuantityApproved = table.Column<int>(type: "int", nullable: false),
                    QuantityPicked = table.Column<int>(type: "int", nullable: false),
                    QuantityIssued = table.Column<int>(type: "int", nullable: false),
                    QuantityDelivered = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PartName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
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
                    table.PrimaryKey("PK_MaterialRequestItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialRequestItems_MaterialRequests_MaterialRequestId",
                        column: x => x.MaterialRequestId,
                        principalTable: "MaterialRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialRequestItems_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1147));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1149));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1151));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1152));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1153));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1155));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1264), new DateTime(2023, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1248) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1267), new DateTime(2024, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1266) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1185));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1186));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1188));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1189));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1202));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1215));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1217));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1218));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(1219));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(937));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(939));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(970));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 13, 44, 23, 337, DateTimeKind.Local).AddTicks(972));

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequestItems_MaterialRequestId",
                table: "MaterialRequestItems",
                column: "MaterialRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequestItems_PartId",
                table: "MaterialRequestItems",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequests_MRNumber",
                table: "MaterialRequests",
                column: "MRNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequests_ServiceOrderId",
                table: "MaterialRequests",
                column: "ServiceOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialRequestItems");

            migrationBuilder.DropTable(
                name: "MaterialRequests");

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
        }
    }
}
