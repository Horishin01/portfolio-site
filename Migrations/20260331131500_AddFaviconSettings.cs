using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PortfolioSite.Data;

#nullable disable

namespace PortfolioSite.Migrations;

[DbContext(typeof(PortfolioDbContext))]
[Migration("20260331131500_AddFaviconSettings")]
public partial class AddFaviconSettings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FaviconSrc",
            table: "portfolio_contents",
            type: "longtext",
            nullable: false,
            defaultValue: "")
            .Annotation("MySql:CharSet", "utf8mb4");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FaviconSrc",
            table: "portfolio_contents");
    }
}
