using System;
using AiCv.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCv.Api.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260505093000_RestructureJobOfferAnalysis")]
public partial class RestructureJobOfferAnalysis : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Opportunities_Users_UserId",
            table: "Opportunities");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Opportunities",
            table: "Opportunities");

        migrationBuilder.RenameTable(
            name: "Opportunities",
            newName: "JobOffers");

        migrationBuilder.RenameIndex(
            name: "IX_Opportunities_UserId",
            table: "JobOffers",
            newName: "IX_JobOffers_UserId");

        migrationBuilder.RenameColumn(
            name: "Company",
            table: "JobOffers",
            newName: "CompanyName");

        migrationBuilder.DropColumn(
            name: "ExtractedKeywords",
            table: "JobOffers");

        migrationBuilder.DropColumn(
            name: "ExtractedSkills",
            table: "JobOffers");

        migrationBuilder.AddColumn<string>(
            name: "AnalysisStatus",
            table: "JobOffers",
            type: "text",
            nullable: false,
            defaultValue: "pending");

        migrationBuilder.AddColumn<DateTime>(
            name: "UpdatedAt",
            table: "JobOffers",
            type: "timestamp with time zone",
            nullable: false,
            defaultValueSql: "CURRENT_TIMESTAMP");

        migrationBuilder.AddPrimaryKey(
            name: "PK_JobOffers",
            table: "JobOffers",
            column: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_JobOffers_Users_UserId",
            table: "JobOffers",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.CreateTable(
            name: "JobOfferAnalyses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                JobOfferId = table.Column<Guid>(type: "uuid", nullable: false),
                ExtractedSkills = table.Column<string>(type: "text", nullable: false),
                ExtractedKeywords = table.Column<string>(type: "text", nullable: false),
                ExtractedResponsibilities = table.Column<string>(type: "text", nullable: false),
                DetectedExperienceLevel = table.Column<string>(type: "text", nullable: false),
                DetectedLocation = table.Column<string>(type: "text", nullable: false),
                DetectedContractType = table.Column<string>(type: "text", nullable: false),
                DetectedTechnologies = table.Column<string>(type: "text", nullable: false),
                AnalysisSummary = table.Column<string>(type: "text", nullable: false),
                RawAnalysisJson = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_JobOfferAnalyses", x => x.Id);
                table.ForeignKey(
                    name: "FK_JobOfferAnalyses_JobOffers_JobOfferId",
                    column: x => x.JobOfferId,
                    principalTable: "JobOffers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_JobOfferAnalyses_JobOfferId",
            table: "JobOfferAnalyses",
            column: "JobOfferId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "JobOfferAnalyses");

        migrationBuilder.DropForeignKey(
            name: "FK_JobOffers_Users_UserId",
            table: "JobOffers");

        migrationBuilder.DropPrimaryKey(
            name: "PK_JobOffers",
            table: "JobOffers");

        migrationBuilder.DropColumn(
            name: "AnalysisStatus",
            table: "JobOffers");

        migrationBuilder.DropColumn(
            name: "UpdatedAt",
            table: "JobOffers");

        migrationBuilder.RenameTable(
            name: "JobOffers",
            newName: "Opportunities");

        migrationBuilder.RenameIndex(
            name: "IX_JobOffers_UserId",
            table: "Opportunities",
            newName: "IX_Opportunities_UserId");

        migrationBuilder.RenameColumn(
            name: "CompanyName",
            table: "Opportunities",
            newName: "Company");

        migrationBuilder.AddColumn<string>(
            name: "ExtractedKeywords",
            table: "Opportunities",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "ExtractedSkills",
            table: "Opportunities",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Opportunities",
            table: "Opportunities",
            column: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Opportunities_Users_UserId",
            table: "Opportunities",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
