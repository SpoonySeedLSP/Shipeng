using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PearAdmin.AbpTemplate.EntityFrameworkCore.Migrations
{
    public partial class AddTable_Business_Enterprise : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Business_Enterprise",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    HostId = table.Column<int>(type: "int", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    CreditCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LegalRepresentative = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegisterDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegisteredCapital = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContributedCapital = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegisteredCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxpayerIdentificationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnterpriseType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessTerm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QualificationTaxpayer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EachDistrict = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationAuthority = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScopeBusiness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyProfile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyInformation = table.Column<string>(type: "nvarchar(max)", maxLength: 98304, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Business_Enterprise", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Business_Enterprise");
        }
    }
}
