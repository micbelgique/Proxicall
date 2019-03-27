using Microsoft.EntityFrameworkCore.Migrations;

namespace Proxicall.CRM.Migrations
{
    public partial class RenamedRefLeadToContact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Company_Lead_RefLeadId",
                table: "Company");

            migrationBuilder.RenameColumn(
                name: "RefLeadId",
                table: "Company",
                newName: "ContactId");

            migrationBuilder.RenameIndex(
                name: "IX_Company_RefLeadId",
                table: "Company",
                newName: "IX_Company_ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_Company_Lead_ContactId",
                table: "Company",
                column: "ContactId",
                principalTable: "Lead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Company_Lead_ContactId",
                table: "Company");

            migrationBuilder.RenameColumn(
                name: "ContactId",
                table: "Company",
                newName: "RefLeadId");

            migrationBuilder.RenameIndex(
                name: "IX_Company_ContactId",
                table: "Company",
                newName: "IX_Company_RefLeadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Company_Lead_RefLeadId",
                table: "Company",
                column: "RefLeadId",
                principalTable: "Lead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
