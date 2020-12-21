using Microsoft.EntityFrameworkCore.Migrations;

namespace Brighid.Identity.Migrations
{
    public partial class ApplicationEncryptedSecret : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncryptedSecret",
                table: "Applications",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptedSecret",
                table: "Applications");
        }
    }
}
