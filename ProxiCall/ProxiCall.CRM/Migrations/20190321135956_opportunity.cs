using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Proxicall.CRM.Migrations
{
    public partial class opportunity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Opportunity",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    OwnerId = table.Column<string>(nullable: false),
                    LeadId = table.Column<string>(nullable: false),
                    ProductId = table.Column<string>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    EstimatedCloseDate = table.Column<DateTime>(nullable: true),
                    Comments = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Confidence = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opportunity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opportunity_Lead_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Lead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Opportunity_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Opportunity_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Opportunity_LeadId",
                table: "Opportunity",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunity_OwnerId",
                table: "Opportunity",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunity_ProductId",
                table: "Opportunity",
                column: "ProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Opportunity");
        }
    }
}
