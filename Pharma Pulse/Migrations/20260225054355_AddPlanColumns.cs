using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharma_Pulse.Migrations
{
    public partial class AddPlanColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Add only new columns (DO NOT create table)

            migrationBuilder.AddColumn<string>(
                name: "PrintFormat",
                table: "Pharmacies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlanName",
                table: "Pharmacies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PlanPrice",
                table: "Pharmacies",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlanValidTill",
                table: "Pharmacies",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.Now);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Pharmacies",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrintFormat",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "PlanName",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "PlanPrice",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "PlanValidTill",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Pharmacies");
        }
    }
}