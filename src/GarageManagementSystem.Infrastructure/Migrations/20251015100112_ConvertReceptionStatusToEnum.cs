using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertReceptionStatusToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add temporary column
            migrationBuilder.AddColumn<int>(
                name: "StatusTemp",
                table: "CustomerReceptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Step 2: Convert existing string values to enum integers
            // Pending = 0, Assigned = 1, InProgress = 2, Completed = 3, Cancelled = 4
            migrationBuilder.Sql(@"
                UPDATE CustomerReceptions 
                SET StatusTemp = CASE 
                    WHEN Status = 'Pending' OR Status = 'Chờ Kiểm Tra' THEN 0
                    WHEN Status = 'Assigned' OR Status = 'Đã Phân Công' THEN 1
                    WHEN Status = 'InProgress' OR Status = 'Đang Kiểm Tra' THEN 2
                    WHEN Status = 'Completed' OR Status = 'Đã Hoàn Thành' THEN 3
                    WHEN Status = 'Cancelled' OR Status = 'Đã Hủy' THEN 4
                    ELSE 0
                END
            ");

            // Step 3: Drop old column
            migrationBuilder.DropColumn(
                name: "Status",
                table: "CustomerReceptions");

            // Step 4: Rename temp column to Status
            migrationBuilder.RenameColumn(
                name: "StatusTemp",
                table: "CustomerReceptions",
                newName: "Status");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7813));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7815));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7816));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7818));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7819));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7821));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7979), new DateTime(2023, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7959) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7990), new DateTime(2024, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7988) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7865));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7867));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7868));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7869));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7882));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7904));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7905));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7907));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7908));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7482));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7489));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7492));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 17, 1, 11, 678, DateTimeKind.Local).AddTicks(7494));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "CustomerReceptions",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 20)
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
        }
    }
}
