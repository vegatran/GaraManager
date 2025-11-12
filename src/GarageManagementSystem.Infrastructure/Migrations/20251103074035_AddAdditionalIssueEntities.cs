using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalIssueEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdditionalIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ServiceOrderId = table.Column<int>(type: "int", nullable: false),
                    ServiceOrderItemId = table.Column<int>(type: "int", nullable: true),
                    Category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IssueName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Severity = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequiresImmediateAction = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReportedByEmployeeId = table.Column<int>(type: "int", nullable: false),
                    TechnicianNotes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReportedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AdditionalQuotationId = table.Column<int>(type: "int", nullable: true),
                    AdditionalServiceOrderId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_AdditionalIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalIssues_Employees_ReportedByEmployeeId",
                        column: x => x.ReportedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdditionalIssues_ServiceOrderItems_ServiceOrderItemId",
                        column: x => x.ServiceOrderItemId,
                        principalTable: "ServiceOrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AdditionalIssues_ServiceOrders_AdditionalServiceOrderId",
                        column: x => x.AdditionalServiceOrderId,
                        principalTable: "ServiceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AdditionalIssues_ServiceOrders_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "ServiceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdditionalIssues_ServiceQuotations_AdditionalQuotationId",
                        column: x => x.AdditionalQuotationId,
                        principalTable: "ServiceQuotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AdditionalIssuePhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AdditionalIssueId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true),
                    AdditionalIssueId1 = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_AdditionalIssuePhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalIssuePhotos_AdditionalIssues_AdditionalIssueId",
                        column: x => x.AdditionalIssueId,
                        principalTable: "AdditionalIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdditionalIssuePhotos_AdditionalIssues_AdditionalIssueId1",
                        column: x => x.AdditionalIssueId1,
                        principalTable: "AdditionalIssues",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalIssuePhotos_AdditionalIssueId",
                table: "AdditionalIssuePhotos",
                column: "AdditionalIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalIssuePhotos_AdditionalIssueId1",
                table: "AdditionalIssuePhotos",
                column: "AdditionalIssueId1");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalIssues_AdditionalQuotationId",
                table: "AdditionalIssues",
                column: "AdditionalQuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalIssues_AdditionalServiceOrderId",
                table: "AdditionalIssues",
                column: "AdditionalServiceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalIssues_ReportedByEmployeeId",
                table: "AdditionalIssues",
                column: "ReportedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalIssues_ServiceOrderId",
                table: "AdditionalIssues",
                column: "ServiceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalIssues_ServiceOrderItemId",
                table: "AdditionalIssues",
                column: "ServiceOrderItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdditionalIssuePhotos");

            migrationBuilder.DropTable(
                name: "AdditionalIssues");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(7999));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8001));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8003));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8005));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8007));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8009));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8190), new DateTime(2023, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8174) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8197), new DateTime(2024, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8194) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8067));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8069));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8071));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8073));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8091));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8102));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8104));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8106));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(8108));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(7565));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(7569));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(7572));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 10, 55, 39, 561, DateTimeKind.Local).AddTicks(7633));
        }
    }
}
