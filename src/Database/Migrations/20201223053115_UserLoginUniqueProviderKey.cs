using Microsoft.EntityFrameworkCore.Migrations;

namespace Brighid.Identity.Common.Database.Migrations
{
    public partial class UserLoginUniqueProviderKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("set foreign_key_checks=0");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "UserLogins",
                type: "varchar(95) CHARACTER SET utf8mb4",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_UserLogins_LoginProvider_ProviderKey",
                table: "UserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.Sql("set foreign_key_checks=1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_UserLogins_LoginProvider_ProviderKey",
                table: "UserLogins");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "UserLogins",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(95) CHARACTER SET utf8mb4");
        }
    }
}
