using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Brighid.Identity.Migrations
{
    public partial class NumericPrimaryKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationRoles_Applications_ApplicationName",
                table: "ApplicationRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationRoles_Roles_RoleName",
                table: "ApplicationRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Applications",
                table: "Applications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationRoles",
                table: "ApplicationRoles");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationRoles_RoleName",
                table: "ApplicationRoles");

            migrationBuilder.DropColumn(
                name: "ApplicationName",
                table: "ApplicationRoles");

            migrationBuilder.DropColumn(
                name: "RoleName",
                table: "ApplicationRoles");

            migrationBuilder.AddColumn<ulong>(
                name: "Id",
                table: "Roles",
                nullable: false,
                defaultValue: 0ul)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<ulong>(
                name: "Id",
                table: "Applications",
                nullable: false,
                defaultValue: 0ul)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<ulong>(
                name: "ApplicationId",
                table: "ApplicationRoles",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "RoleId",
                table: "ApplicationRoles",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Applications",
                table: "Applications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationRoles",
                table: "ApplicationRoles",
                columns: new[] { "ApplicationId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Name",
                table: "Applications",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoles_RoleId",
                table: "ApplicationRoles",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationRoles_Applications_ApplicationId",
                table: "ApplicationRoles",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationRoles_Roles_RoleId",
                table: "ApplicationRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationRoles_Applications_ApplicationId",
                table: "ApplicationRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationRoles_Roles_RoleId",
                table: "ApplicationRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_Name",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Applications",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_Name",
                table: "Applications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationRoles",
                table: "ApplicationRoles");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationRoles_RoleId",
                table: "ApplicationRoles");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "ApplicationRoles");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "ApplicationRoles");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationName",
                table: "ApplicationRoles",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RoleName",
                table: "ApplicationRoles",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Applications",
                table: "Applications",
                column: "Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationRoles",
                table: "ApplicationRoles",
                columns: new[] { "ApplicationName", "RoleName" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoles_RoleName",
                table: "ApplicationRoles",
                column: "RoleName");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationRoles_Applications_ApplicationName",
                table: "ApplicationRoles",
                column: "ApplicationName",
                principalTable: "Applications",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationRoles_Roles_RoleName",
                table: "ApplicationRoles",
                column: "RoleName",
                principalTable: "Roles",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
