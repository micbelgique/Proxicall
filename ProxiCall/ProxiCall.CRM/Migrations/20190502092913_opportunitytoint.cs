using Microsoft.EntityFrameworkCore.Migrations;

namespace ProxiCall.CRM.Migrations
{
    public partial class opportunitytoint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Opportunities",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Confidence",
                table: "Opportunities",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Opportunities",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "Confidence",
                table: "Opportunities",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}
