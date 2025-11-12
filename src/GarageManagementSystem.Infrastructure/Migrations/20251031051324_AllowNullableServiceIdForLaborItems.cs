using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowNullableServiceIdForLaborItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ServiceId",
                table: "ServiceOrderItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7780));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7782));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7783));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7785));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7787));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7788));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(8040), new DateTime(2023, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(8014) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HireDate" },
                values: new object[] { new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(8046), new DateTime(2024, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(8044) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7850));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7852));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7853));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7906));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7929));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7953));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7955));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7957));

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7959));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7412));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7415));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7418));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 12, 13, 20, 883, DateTimeKind.Local).AddTicks(7421));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ServiceId",
                table: "ServiceOrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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
        }
    }
}
