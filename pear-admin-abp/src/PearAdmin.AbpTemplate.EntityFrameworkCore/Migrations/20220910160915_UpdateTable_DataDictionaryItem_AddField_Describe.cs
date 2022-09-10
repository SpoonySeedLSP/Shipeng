using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PearAdmin.AbpTemplate.EntityFrameworkCore.Migrations
{
    public partial class UpdateTable_DataDictionaryItem_AddField_Describe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Describe",
                table: "Resource_DataDictionaryItem",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EnterpriseId",
                table: "Resource_DataDictionaryItem",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "HostId",
                table: "Resource_DataDictionaryItem",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "OrganizationId",
                table: "Business_FileManage",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "EnterpriseId",
                table: "Business_FileManage",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "OrganizationId",
                table: "Business_FileLists",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "FileId",
                table: "Business_FileLists",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");

            migrationBuilder.AlterColumn<long>(
                name: "EnterpriseId",
                table: "Business_FileLists",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "FinalyFilePath",
                table: "Business_FileLists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "Business_FileLists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StorageFileName",
                table: "Business_FileLists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "OrganizationId",
                table: "Business_FileArticleContent",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "EnterpriseId",
                table: "Business_FileArticleContent",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Describe",
                table: "Resource_DataDictionaryItem");

            migrationBuilder.DropColumn(
                name: "EnterpriseId",
                table: "Resource_DataDictionaryItem");

            migrationBuilder.DropColumn(
                name: "HostId",
                table: "Resource_DataDictionaryItem");

            migrationBuilder.DropColumn(
                name: "FinalyFilePath",
                table: "Business_FileLists");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "Business_FileLists");

            migrationBuilder.DropColumn(
                name: "StorageFileName",
                table: "Business_FileLists");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "Business_FileManage",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "EnterpriseId",
                table: "Business_FileManage",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "Business_FileLists",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "FileId",
                table: "Business_FileLists",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");

            migrationBuilder.AlterColumn<int>(
                name: "EnterpriseId",
                table: "Business_FileLists",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "Business_FileArticleContent",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "EnterpriseId",
                table: "Business_FileArticleContent",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
