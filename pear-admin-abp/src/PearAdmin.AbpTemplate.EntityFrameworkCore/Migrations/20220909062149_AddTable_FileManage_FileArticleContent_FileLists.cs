using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PearAdmin.AbpTemplate.EntityFrameworkCore.Migrations
{
    public partial class AddTable_FileManage_FileArticleContent_FileLists : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Business_FileArticleContent",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    HostId = table.Column<int>(type: "int", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    EnterpriseId = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    FileManageId = table.Column<long>(type: "bigint", nullable: false),
                    ClassifyId = table.Column<long>(type: "bigint", nullable: false),
                    ArticleContent = table.Column<string>(type: "nvarchar(max)", maxLength: 245760, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Business_FileArticleContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Business_FileLists",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    HostId = table.Column<int>(type: "int", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    EnterpriseId = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    FileManageId = table.Column<long>(type: "bigint", nullable: false),
                    ClassifyId = table.Column<long>(type: "bigint", nullable: false),
                    FileId = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Bytes = table.Column<byte[]>(type: "varbinary(max)", maxLength: 245760, nullable: true),
                    DirectoryPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Business_FileLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Business_FileManage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    HostId = table.Column<int>(type: "int", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    EnterpriseId = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    FilePublicType = table.Column<int>(type: "int", nullable: false),
                    ClassifyId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Describe = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Viewed = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Business_FileManage", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Business_FileArticleContent");

            migrationBuilder.DropTable(
                name: "Business_FileLists");

            migrationBuilder.DropTable(
                name: "Business_FileManage");
        }
    }
}
