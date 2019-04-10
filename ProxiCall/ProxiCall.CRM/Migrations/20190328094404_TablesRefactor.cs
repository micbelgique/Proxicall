using Microsoft.EntityFrameworkCore.Migrations;

namespace ProxiCall.CRM.Migrations
{
    public partial class TablesRefactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Company_Lead_ContactId",
                table: "Company");

            migrationBuilder.DropForeignKey(
                name: "FK_Lead_Company_CompanyId",
                table: "Lead");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunity_Lead_LeadId",
                table: "Opportunity");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunity_AspNetUsers_OwnerId",
                table: "Opportunity");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunity_Product_ProductId",
                table: "Opportunity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Product",
                table: "Product");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Opportunity",
                table: "Opportunity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lead",
                table: "Lead");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Company",
                table: "Company");

            migrationBuilder.RenameTable(
                name: "Product",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "Opportunity",
                newName: "Opportunities");

            migrationBuilder.RenameTable(
                name: "Lead",
                newName: "Leads");

            migrationBuilder.RenameTable(
                name: "Company",
                newName: "Companies");

            migrationBuilder.RenameIndex(
                name: "IX_Opportunity_ProductId",
                table: "Opportunities",
                newName: "IX_Opportunities_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Opportunity_OwnerId",
                table: "Opportunities",
                newName: "IX_Opportunities_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Opportunity_LeadId",
                table: "Opportunities",
                newName: "IX_Opportunities_LeadId");

            migrationBuilder.RenameIndex(
                name: "IX_Lead_CompanyId",
                table: "Leads",
                newName: "IX_Leads_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Company_ContactId",
                table: "Companies",
                newName: "IX_Companies_ContactId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Opportunities",
                table: "Opportunities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Leads",
                table: "Leads",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Companies",
                table: "Companies",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Leads_ContactId",
                table: "Companies",
                column: "ContactId",
                principalTable: "Leads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Companies_CompanyId",
                table: "Leads",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Leads_LeadId",
                table: "Opportunities",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_AspNetUsers_OwnerId",
                table: "Opportunities",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Products_ProductId",
                table: "Opportunities",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Leads_ContactId",
                table: "Companies");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Companies_CompanyId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Leads_LeadId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_AspNetUsers_OwnerId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Products_ProductId",
                table: "Opportunities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Opportunities",
                table: "Opportunities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Leads",
                table: "Leads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Companies",
                table: "Companies");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Product");

            migrationBuilder.RenameTable(
                name: "Opportunities",
                newName: "Opportunity");

            migrationBuilder.RenameTable(
                name: "Leads",
                newName: "Lead");

            migrationBuilder.RenameTable(
                name: "Companies",
                newName: "Company");

            migrationBuilder.RenameIndex(
                name: "IX_Opportunities_ProductId",
                table: "Opportunity",
                newName: "IX_Opportunity_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Opportunities_OwnerId",
                table: "Opportunity",
                newName: "IX_Opportunity_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Opportunities_LeadId",
                table: "Opportunity",
                newName: "IX_Opportunity_LeadId");

            migrationBuilder.RenameIndex(
                name: "IX_Leads_CompanyId",
                table: "Lead",
                newName: "IX_Lead_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Companies_ContactId",
                table: "Company",
                newName: "IX_Company_ContactId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Product",
                table: "Product",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Opportunity",
                table: "Opportunity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lead",
                table: "Lead",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Company",
                table: "Company",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Company_Lead_ContactId",
                table: "Company",
                column: "ContactId",
                principalTable: "Lead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lead_Company_CompanyId",
                table: "Lead",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunity_Lead_LeadId",
                table: "Opportunity",
                column: "LeadId",
                principalTable: "Lead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunity_AspNetUsers_OwnerId",
                table: "Opportunity",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunity_Product_ProductId",
                table: "Opportunity",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
