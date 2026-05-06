using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myhijyenProje.Migrations
{
    /// <inheritdoc />
    public partial class AddCorporateDiscountSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CorporateApprovedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorporateApprovedBy",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorporateEmailDomain",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCorporateApproved",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CorporateDiscountRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailDomain = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    RequiresAdminApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporateDiscountRules", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorporateDiscountRules");

            migrationBuilder.DropColumn(
                name: "CorporateApprovedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CorporateApprovedBy",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CorporateEmailDomain",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsCorporateApproved",
                table: "AspNetUsers");
        }
    }
}
