using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedbackChannels",
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
                    table.PrimaryKey("PK_FeedbackChannels", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomerFeedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    ServiceOrderId = table.Column<int>(type: "int", nullable: true),
                    Source = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rating = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Topic = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActionTaken = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FollowUpDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FollowUpById = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Score = table.Column<int>(type: "int", nullable: true),
                    FeedbackChannelId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_CustomerFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerFeedbacks_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CustomerFeedbacks_Employees_FollowUpById",
                        column: x => x.FollowUpById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CustomerFeedbacks_FeedbackChannels_FeedbackChannelId",
                        column: x => x.FeedbackChannelId,
                        principalTable: "FeedbackChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CustomerFeedbacks_ServiceOrders_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "ServiceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomerFeedbackAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CustomerFeedbackId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FilePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_CustomerFeedbackAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerFeedbackAttachments_CustomerFeedbacks_CustomerFeedba~",
                        column: x => x.CustomerFeedbackId,
                        principalTable: "CustomerFeedbacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.InsertData(
                table: "FeedbackChannels",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsActive", "IsDeleted", "IsSystem", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "HOTLINE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), null, null, null, "Gọi hotline/điện thoại", 1, true, false, true, "Hotline", null, null },
                    { 2, "IN_PERSON", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), null, null, null, "Khách phản hồi trực tiếp tại gara", 2, true, false, true, "Tại gara", null, null },
                    { 3, "EMAIL", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), null, null, null, "Phản hồi qua email", 3, true, false, true, "Email", null, null },
                    { 4, "APP", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), null, null, null, "Phản hồi qua app", 4, true, false, true, "Ứng dụng di động", null, null },
                    { 5, "SOCIAL", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), null, null, null, "Phản hồi Facebook/Zalo...", 5, true, false, true, "Mạng xã hội", null, null },
                    { 6, "SURVEY", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), null, null, null, "Khảo sát hậu mãi", 6, true, false, true, "Survey hậu mãi", null, null }
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedbackAttachments_CustomerFeedbackId",
                table: "CustomerFeedbackAttachments",
                column: "CustomerFeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedbacks_CustomerId_ServiceOrderId_Status",
                table: "CustomerFeedbacks",
                columns: new[] { "CustomerId", "ServiceOrderId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedbacks_FeedbackChannelId",
                table: "CustomerFeedbacks",
                column: "FeedbackChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedbacks_FollowUpById",
                table: "CustomerFeedbacks",
                column: "FollowUpById");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedbacks_FollowUpDate",
                table: "CustomerFeedbacks",
                column: "FollowUpDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedbacks_ServiceOrderId",
                table: "CustomerFeedbacks",
                column: "ServiceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedbacks_Source",
                table: "CustomerFeedbacks",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackChannels_Code",
                table: "FeedbackChannels",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerFeedbackAttachments");

            migrationBuilder.DropTable(
                name: "CustomerFeedbacks");

            migrationBuilder.DropTable(
                name: "FeedbackChannels");

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
        }
    }
}
