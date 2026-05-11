using System;
using AiCv.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCv.Api.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260511090000_AddRichJobOfferAnalysisFields")]
public partial class AddRichJobOfferAnalysisFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CandidateRisks",
            table: "JobOfferAnalyses",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<double>(
            name: "ConfidenceScore",
            table: "JobOfferAnalyses",
            type: "double precision",
            nullable: false,
            defaultValue: 0.0);

        migrationBuilder.AddColumn<string>(
            name: "CvFocusPoints",
            table: "JobOfferAnalyses",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<int>(
            name: "MatchScoreEstimation",
            table: "JobOfferAnalyses",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "MustHaveRequirements",
            table: "JobOfferAnalyses",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "NiceToHaveRequirements",
            table: "JobOfferAnalyses",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "ReasoningSummary",
            table: "JobOfferAnalyses",
            type: "text",
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CandidateRisks",
            table: "JobOfferAnalyses");

        migrationBuilder.DropColumn(
            name: "ConfidenceScore",
            table: "JobOfferAnalyses");

        migrationBuilder.DropColumn(
            name: "CvFocusPoints",
            table: "JobOfferAnalyses");

        migrationBuilder.DropColumn(
            name: "MatchScoreEstimation",
            table: "JobOfferAnalyses");

        migrationBuilder.DropColumn(
            name: "MustHaveRequirements",
            table: "JobOfferAnalyses");

        migrationBuilder.DropColumn(
            name: "NiceToHaveRequirements",
            table: "JobOfferAnalyses");

        migrationBuilder.DropColumn(
            name: "ReasoningSummary",
            table: "JobOfferAnalyses");
    }
}
