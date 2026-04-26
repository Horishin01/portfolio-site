using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioSite.Migrations
{
    /// <inheritdoc />
    public partial class AddAdsensePublishCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdsenseBodyScript",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AdsenseHeadScript",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "AdsenseIsEnabled",
                table: "portfolio_contents",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AdsensePublisherId",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdsenseBodyScript",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "AdsenseHeadScript",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "AdsenseIsEnabled",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "AdsensePublisherId",
                table: "portfolio_contents");
        }
    }
}
