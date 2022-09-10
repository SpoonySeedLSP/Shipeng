using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PearAdmin.AbpTemplate.EntityFrameworkCore.Migrations
{
    public partial class EditTable_BinaryObject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Common_BinaryObject",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Common_BinaryObject",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Common_BinaryObject",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Common_BinaryObject");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Common_BinaryObject");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Common_BinaryObject");
        }
    }
}
