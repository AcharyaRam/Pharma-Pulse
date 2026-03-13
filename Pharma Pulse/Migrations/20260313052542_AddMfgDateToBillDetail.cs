using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace Pharma_Pulse.Migrations
{
    public partial class AddMfgDateToBillDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MfgDate",
                table: "BillDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2000, 1, 1));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MfgDate",
                table: "BillDetails");
        }
    }
}