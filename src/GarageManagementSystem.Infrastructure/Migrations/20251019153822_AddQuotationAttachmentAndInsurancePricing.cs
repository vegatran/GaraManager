using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationAttachmentAndInsurancePricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceApprovedDiscountAmount",
                table: "ServiceQuotations",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceApprovedSubTotal",
                table: "ServiceQuotations",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceApprovedTaxAmount",
                table: "ServiceQuotations",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceApprovedTotalAmount",
                table: "ServiceQuotations",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceApprovalNotes",
                table: "QuotationItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceApprovedSubTotal",
                table: "QuotationItems",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceApprovedTotalAmount",
                table: "QuotationItems",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceApprovedUnitPrice",
                table: "QuotationItems",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceApprovedVATAmount",
                table: "QuotationItems",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuotationAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ServiceQuotationId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FilePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    AttachmentType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsInsuranceDocument = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UploadedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UploadedById = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_QuotationAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationAttachments_Employees_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuotationAttachments_ServiceQuotations_ServiceQuotationId",
                        column: x => x.ServiceQuotationId,
                        principalTable: "ServiceQuotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1369));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1373));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1377));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1380));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1384));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1387));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(2031), new DateTime(2023, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(2004) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(2042), new DateTime(2024, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(2037) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1461));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1465));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1468));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1472));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1486));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1521));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1524));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1527));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(1530));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(843));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(849));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(854));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 19, 22, 38, 18, 664, DateTimeKind.Local).AddTicks(859));

            migrationBuilder.CreateIndex(
                name: "IX_QuotationAttachments_ServiceQuotationId",
                table: "QuotationAttachments",
                column: "ServiceQuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationAttachments_UploadedById",
                table: "QuotationAttachments",
                column: "UploadedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuotationAttachments");

            migrationBuilder.DropColumn(
                name: "InsuranceApprovedDiscountAmount",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "InsuranceApprovedSubTotal",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "InsuranceApprovedTaxAmount",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "InsuranceApprovedTotalAmount",
                table: "ServiceQuotations");

            migrationBuilder.DropColumn(
                name: "InsuranceApprovalNotes",
                table: "QuotationItems");

            migrationBuilder.DropColumn(
                name: "InsuranceApprovedSubTotal",
                table: "QuotationItems");

            migrationBuilder.DropColumn(
                name: "InsuranceApprovedTotalAmount",
                table: "QuotationItems");

            migrationBuilder.DropColumn(
                name: "InsuranceApprovedUnitPrice",
                table: "QuotationItems");

            migrationBuilder.DropColumn(
                name: "InsuranceApprovedVATAmount",
                table: "QuotationItems");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1109));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1111));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1112));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1113));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1115));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1116));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1260), new DateTime(2023, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1239) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1264), new DateTime(2024, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1262) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1145));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1147));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1148));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1150));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1159));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1185));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1187));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1189));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(1190));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(797));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(802));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(804));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 10, 19, 13, 635, DateTimeKind.Local).AddTicks(806));
        }
    }
}
