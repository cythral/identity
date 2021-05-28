using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable IDE0050

namespace Brighid.Identity.Common.Database.Migrations
{
    public partial class LoginProviders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NormalizedName",
                table: "Roles",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConcurrencyStamp",
                table: "Roles",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "LoginProviders",
                columns: table => new
                {
                    Name = table.Column<string>(type: "varchar(95) CHARACTER SET utf8mb4", nullable: false),
                    Description = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    UserIdField = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    SpaceDelimitedScopes = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    AuthType = table.Column<int>(type: "int", nullable: false),
                    AuthorizeUrl = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    TokenUrl = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    UserInfoUrl = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    ImageUrl = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginProviders", x => x.Name);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginProviders");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedName",
                table: "Roles",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ConcurrencyStamp",
                table: "Roles",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4");
        }
    }
}
