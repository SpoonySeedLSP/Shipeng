using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PearAdmin.AbpTemplate.EntityFrameworkCore.Migrations
{
    public partial class UpdateUser_AddField_LastLoginTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginTime",
                table: "AbpUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLoginTime",
                table: "AbpUsers");
        }
    }
}
