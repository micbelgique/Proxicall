using Microsoft.EntityFrameworkCore.Migrations;

namespace Proxicall.CRM.Migrations
{
    public partial class leadref : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lead_Company_CompanyId",
                table: "Lead");

            migrationBuilder.DropIndex(
                name: "IX_Lead_CompanyId",
                table: "Lead");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyId",
                table: "Lead",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefLeadId",
                table: "Company",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_RefLeadId",
                table: "Company",
                column: "RefLeadId",
                unique: true,
                filter: "[RefLeadId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Company_Lead_RefLeadId",
                table: "Company",
                column: "RefLeadId",
                principalTable: "Lead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Company_Lead_RefLeadId",
                table: "Company");

            migrationBuilder.DropIndex(
                name: "IX_Company_RefLeadId",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "RefLeadId",
                table: "Company");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyId",
                table: "Lead",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lead_CompanyId",
                table: "Lead",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lead_Company_CompanyId",
                table: "Lead",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
