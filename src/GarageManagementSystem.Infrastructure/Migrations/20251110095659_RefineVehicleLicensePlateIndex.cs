using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefineVehicleLicensePlateIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicensePlate_IsDeleted",
                table: "Vehicles");

            migrationBuilder.AddColumn<string>(
                name: "LicensePlateUnique",
                table: "Vehicles",
                type: "varchar(20)",
                nullable: true,
                computedColumnSql: "CASE WHEN (`IsDeleted` = 0) THEN `LicensePlate` ELSE NULL END",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlateUnique",
                table: "Vehicles",
                column: "LicensePlateUnique",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicensePlateUnique",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "LicensePlateUnique",
                table: "Vehicles");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate_IsDeleted",
                table: "Vehicles",
                columns: new[] { "LicensePlate", "IsDeleted" },
                unique: true);
        }
    }
}
