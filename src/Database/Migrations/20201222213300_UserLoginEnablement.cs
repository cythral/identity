using System;

using Microsoft.EntityFrameworkCore.Migrations;

namespace Brighid.Identity.Common.Database.Migrations
{
    public partial class UserLoginEnablement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_UserLogins_Users_UserId", "UserLogins");

            migrationBuilder.DropIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLogins_Users_UserId",
                table: "UserLogins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "UserLogins",
                type: "varchar(95) CHARACTER SET utf8mb4",
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_UserLogins_UserId_LoginProvider",
                table: "UserLogins",
                columns: new[] { "UserId", "LoginProvider" });

            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "UserLogins",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.DropColumn("Id", "UserClaims");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "UserClaims",
                type: "binary(16)",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_UserLogins_Users_UserId", "UserLogins");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_UserLogins_UserId_LoginProvider",
                table: "UserLogins");

            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "UserLogins");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "UserLogins",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(95) CHARACTER SET utf8mb4");

            migrationBuilder.DropColumn("Id", "UserClaims");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserClaims",
                type: "int",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLogins_Users_UserId",
                table: "UserLogins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
