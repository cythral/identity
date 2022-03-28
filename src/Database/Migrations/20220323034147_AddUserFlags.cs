using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brighid.Identity.Common.Database.Migrations
{
    public partial class AddUserFlags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Flags",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Flags",
                table: "Users");
        }
    }
}
