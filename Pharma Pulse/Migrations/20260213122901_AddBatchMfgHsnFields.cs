using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharma_Pulse.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchMfgHsnFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BatchNo",
                table: "Medicines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HsnSac",
                table: "Medicines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "MfgDate",
                table: "Medicines",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatchNo",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "HsnSac",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "MfgDate",
                table: "Medicines");
        }
    }
}
