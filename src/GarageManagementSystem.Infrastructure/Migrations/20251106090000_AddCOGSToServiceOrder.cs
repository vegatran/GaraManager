using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCOGSToServiceOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalCOGS",
                table: "ServiceOrders",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "COGSCalculationMethod",
                table: "ServiceOrders",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "FIFO")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "COGSCalculationDate",
                table: "ServiceOrders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "COGSBreakdown",
                table: "ServiceOrders",
                type: "varchar(5000)",
                maxLength: 5000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalCOGS",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "COGSCalculationMethod",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "COGSCalculationDate",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "COGSBreakdown",
                table: "ServiceOrders");
        }
    }
}

