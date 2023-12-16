using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalkFusion.Data.Migrations
{
    public partial class AddFileAttr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "File",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "File",
                table: "Comments");
        }
    }
}
