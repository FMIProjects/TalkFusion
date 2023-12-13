using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalkFusion.Data.Migrations
{
    public partial class UpdateUserGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserGroups",
                table: "UserGroups");

            migrationBuilder.AddColumn<bool>(
                name: "IsModerator",
                table: "UserGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserGroups",
                table: "UserGroups",
                columns: new[] { "Id", "UserId", "GroupId", "IsModerator" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserGroups",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "IsModerator",
                table: "UserGroups");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserGroups",
                table: "UserGroups",
                columns: new[] { "Id", "UserId", "GroupId" });
        }
    }
}
