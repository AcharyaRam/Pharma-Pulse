using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharma_Pulse.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacyExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FSSAINumber",
                table: "Pharmacies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseType",
                table: "Pharmacies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PANNumber",
                table: "Pharmacies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StateCode",
                table: "Pharmacies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FSSAINumber",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "LicenseType",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "PANNumber",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "StateCode",
                table: "Pharmacies");
        }
    }
}
