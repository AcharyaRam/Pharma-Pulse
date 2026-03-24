using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharma_Pulse.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacyMultiTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PharmacyId",
                table: "Sales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Pharmacies",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Pharmacies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Pharmacies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PharmacyId",
                table: "Medicines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PharmacyId",
                table: "GstSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PharmacyId",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PharmacyId",
                table: "Bills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PharmacyId",
                table: "BillDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "GstSettings");

            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "BillDetails");
        }
    }
}
