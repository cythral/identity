using System;

using Microsoft.EntityFrameworkCore.Migrations;

namespace Brighid.Identity.Common.Database.Migrations
{
    public partial class OpenIddict3Upgrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "OpenIddictTokens",
                type: "varchar(50) CHARACTER SET utf8mb4",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(25) CHARACTER SET utf8mb4",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "OpenIddictTokens",
                type: "varchar(400) CHARACTER SET utf8mb4",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(450) CHARACTER SET utf8mb4",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "OpenIddictTokens",
                type: "varchar(50) CHARACTER SET utf8mb4",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(25) CHARACTER SET utf8mb4",
                oldMaxLength: 25);

            migrationBuilder.AddColumn<DateTime>(
                name: "RedemptionDate",
                table: "OpenIddictTokens",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "OpenIddictScopes",
                type: "varchar(200) CHARACTER SET utf8mb4",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200) CHARACTER SET utf8mb4",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "Descriptions",
                table: "OpenIddictScopes",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayNames",
                table: "OpenIddictScopes",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "OpenIddictAuthorizations",
                type: "varchar(50) CHARACTER SET utf8mb4",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(25) CHARACTER SET utf8mb4",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "OpenIddictAuthorizations",
                type: "varchar(400) CHARACTER SET utf8mb4",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(450) CHARACTER SET utf8mb4",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "OpenIddictAuthorizations",
                type: "varchar(50) CHARACTER SET utf8mb4",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(25) CHARACTER SET utf8mb4",
                oldMaxLength: 25);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "OpenIddictAuthorizations",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "OpenIddictApplications",
                type: "varchar(50) CHARACTER SET utf8mb4",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(25) CHARACTER SET utf8mb4",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<string>(
                name: "ConsentType",
                table: "OpenIddictApplications",
                type: "varchar(50) CHARACTER SET utf8mb4",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                table: "OpenIddictApplications",
                type: "varchar(100) CHARACTER SET utf8mb4",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100) CHARACTER SET utf8mb4",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "DisplayNames",
                table: "OpenIddictApplications",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Requirements",
                table: "OpenIddictApplications",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RedemptionDate",
                table: "OpenIddictTokens");

            migrationBuilder.DropColumn(
                name: "Descriptions",
                table: "OpenIddictScopes");

            migrationBuilder.DropColumn(
                name: "DisplayNames",
                table: "OpenIddictScopes");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropColumn(
                name: "DisplayNames",
                table: "OpenIddictApplications");

            migrationBuilder.DropColumn(
                name: "Requirements",
                table: "OpenIddictApplications");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "OpenIddictTokens",
                type: "varchar(25) CHARACTER SET utf8mb4",
                maxLength: 25,
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "varchar(50) CHARACTER SET utf8mb4",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "OpenIddictTokens",
                type: "varchar(450) CHARACTER SET utf8mb4",
                maxLength: 450,
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "varchar(400) CHARACTER SET utf8mb4",
                oldMaxLength: 400,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "OpenIddictTokens",
                type: "varchar(25) CHARACTER SET utf8mb4",
                maxLength: 25,
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "varchar(50) CHARACTER SET utf8mb4",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "OpenIddictScopes",
                type: "varchar(200) CHARACTER SET utf8mb4",
                maxLength: 200,
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "varchar(200) CHARACTER SET utf8mb4",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "OpenIddictAuthorizations",
                type: "varchar(25) CHARACTER SET utf8mb4",
                maxLength: 25,
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "varchar(50) CHARACTER SET utf8mb4",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "OpenIddictAuthorizations",
                type: "varchar(450) CHARACTER SET utf8mb4",
                maxLength: 450,
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "varchar(400) CHARACTER SET utf8mb4",
                oldMaxLength: 400,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "OpenIddictAuthorizations",
                type: "varchar(25) CHARACTER SET utf8mb4",
                maxLength: 25,
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "varchar(50) CHARACTER SET utf8mb4",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "OpenIddictApplications",
                type: "varchar(25) CHARACTER SET utf8mb4",
                maxLength: 25,
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "varchar(50) CHARACTER SET utf8mb4",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConsentType",
                table: "OpenIddictApplications",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50) CHARACTER SET utf8mb4",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                table: "OpenIddictApplications",
                type: "varchar(100) CHARACTER SET utf8mb4",
                maxLength: 100,
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "varchar(100) CHARACTER SET utf8mb4",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
