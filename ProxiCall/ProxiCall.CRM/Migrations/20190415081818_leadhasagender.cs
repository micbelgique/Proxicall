using Microsoft.EntityFrameworkCore.Migrations;

namespace ProxiCall.CRM.Migrations
{
    public partial class leadhasagender : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Leads",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Leads");
        }
    }
}
