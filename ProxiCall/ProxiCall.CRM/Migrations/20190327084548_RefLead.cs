using Microsoft.EntityFrameworkCore.Migrations;

namespace ProxiCall.CRM.Migrations
{
    public partial class RefLead : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Company_RefLeadId",
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

            migrationBuilder.CreateIndex(
                name: "IX_Company_RefLeadId",
                table: "Company",
                column: "RefLeadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lead_Company_CompanyId",
                table: "Lead",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lead_Company_CompanyId",
                table: "Lead");

            migrationBuilder.DropIndex(
                name: "IX_Lead_CompanyId",
                table: "Lead");

            migrationBuilder.DropIndex(
                name: "IX_Company_RefLeadId",
                table: "Company");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyId",
                table: "Lead",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_RefLeadId",
                table: "Company",
                column: "RefLeadId",
                unique: true,
                filter: "[RefLeadId] IS NOT NULL");
        }
    }
}
