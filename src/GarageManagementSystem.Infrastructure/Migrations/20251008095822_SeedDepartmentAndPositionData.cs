using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDepartmentAndPositionData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1816), null, null, null, "Bộ phận dịch vụ sửa chữa và bảo dưỡng xe", true, false, "Dịch Vụ", null, null },
                    { 2, new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1818), null, null, null, "Bộ phận quản lý phụ tùng và linh kiện", true, false, "Phụ Tùng", null, null },
                    { 3, new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1819), null, null, null, "Bộ phận hành chính và quản lý", true, false, "Hành Chính", null, null },
                    { 4, new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1821), null, null, null, "Bộ phận kế toán và tài chính", true, false, "Kế Toán", null, null },
                    { 5, new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1822), null, null, null, "Bộ phận chăm sóc và hỗ trợ khách hàng", true, false, "Chăm Sóc Khách Hàng", null, null },
                    { 6, new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1823), null, null, null, "Bộ phận quản lý và điều hành", true, false, "Quản Lý", null, null }
                });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1979), new DateTime(2023, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1962) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1984), new DateTime(2024, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1981) });

            migrationBuilder.InsertData(
                table: "Positions",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1919), null, null, null, "Thực hiện sửa chữa và bảo dưỡng xe", true, false, "Kỹ Thuật Viên", null, null },
                    { 2, new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1921), null, null, null, "Kỹ thuật viên có kinh nghiệm cao", true, false, "Kỹ Thuật Viên Cao Cấp", null, null },
                    { 3, new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1922), null, null, null, "Quản lý và tư vấn phụ tùng", true, false, "Chuyên Viên Phụ Tùng", null, null },
                    { 4, new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1924), null, null, null, "Tư vấn và hỗ trợ khách hàng", true, false, "Tư Vấn Dịch Vụ", null, null },
                    { 5, new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1925), null, null, null, "Tiếp đón và hỗ trợ khách hàng", true, false, "Lễ Tân", null, null },
                    { 6, new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1926), null, null, null, "Xử lý công việc kế toán", true, false, "Kế Toán", null, null },
                    { 7, new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1927), null, null, null, "Quản lý và điều hành", true, false, "Quản Lý", null, null },
                    { 8, new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1929), null, null, null, "Hỗ trợ công việc quản lý", true, false, "Trợ Lý", null, null },
                    { 9, new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1930), null, null, null, "Giám sát hoạt động sửa chữa", true, false, "Giám Sát", null, null }
                });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1576));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1578));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1580));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 8, 16, 58, 21, 960, DateTimeKind.Local).AddTicks(1582));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 8, 16, 53, 28, 495, DateTimeKind.Local).AddTicks(3776), new DateTime(2023, 10, 8, 16, 53, 28, 495, DateTimeKind.Local).AddTicks(3764) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 8, 16, 53, 28, 495, DateTimeKind.Local).AddTicks(3792), new DateTime(2024, 10, 8, 16, 53, 28, 495, DateTimeKind.Local).AddTicks(3790) });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 8, 16, 53, 28, 495, DateTimeKind.Local).AddTicks(3466));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 8, 16, 53, 28, 495, DateTimeKind.Local).AddTicks(3469));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 8, 16, 53, 28, 495, DateTimeKind.Local).AddTicks(3471));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 8, 16, 53, 28, 495, DateTimeKind.Local).AddTicks(3473));
        }
    }
}
