using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityControlAndHandoverFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HandoverDate",
                table: "ServiceOrders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HandoverLocation",
                table: "ServiceOrders",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "QCFailedCount",
                table: "ServiceOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalActualHours",
                table: "ServiceOrders",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReworkHours",
                table: "ServiceOrderItems",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QualityControls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ServiceOrderId = table.Column<int>(type: "int", nullable: false),
                    QCInspectorId = table.Column<int>(type: "int", nullable: true),
                    QCDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    QCResult = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QCNotes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReworkRequired = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReworkNotes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QCCompletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_QualityControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityControls_Employees_QCInspectorId",
                        column: x => x.QCInspectorId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_QualityControls_ServiceOrders_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "ServiceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QCChecklistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QualityControlId = table.Column<int>(type: "int", nullable: false),
                    ChecklistItemName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsChecked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Result = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_QCChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QCChecklistItems_QualityControls_QualityControlId",
                        column: x => x.QualityControlId,
                        principalTable: "QualityControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8619));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8621));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8623));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8625));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8626));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8628));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8772), new DateTime(2023, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8744) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8786), new DateTime(2024, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8782) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8668));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8671));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8672));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8674));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8679));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8704));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8705));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8707));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8708));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8326));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8330));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8332));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 5, 15, 3, 10, 220, DateTimeKind.Local).AddTicks(8335));

            migrationBuilder.CreateIndex(
                name: "IX_QCChecklistItems_QualityControlId",
                table: "QCChecklistItems",
                column: "QualityControlId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityControls_QCInspectorId",
                table: "QualityControls",
                column: "QCInspectorId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityControls_ServiceOrderId",
                table: "QualityControls",
                column: "ServiceOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QCChecklistItems");

            migrationBuilder.DropTable(
                name: "QualityControls");

            migrationBuilder.DropColumn(
                name: "HandoverDate",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "HandoverLocation",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "QCFailedCount",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "TotalActualHours",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "ReworkHours",
                table: "ServiceOrderItems");

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
        }
    }
}
