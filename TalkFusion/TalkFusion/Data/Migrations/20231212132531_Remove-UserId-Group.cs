using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalkFusion.Data.Migrations
{
    public partial class RemoveUserIdGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
            name: "UserId",
            table: "Groups");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
