using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowDuplicateLicensePlateWhenDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate_IsDeleted",
                table: "Vehicles",
                columns: new[] { "LicensePlate", "IsDeleted" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicensePlate_IsDeleted",
                table: "Vehicles");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles",
                column: "LicensePlate",
                unique: true);
        }
    }
}
