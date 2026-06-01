using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCv.OpportunityService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialOpportunityServiceCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KeycloakId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobOffers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    AnalysisStatus = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobOffers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    MustHaveRequirements = table.Column<string>(type: "text", nullable: false),
                    NiceToHaveRequirements = table.Column<string>(type: "text", nullable: false),
                    CvFocusPoints = table.Column<string>(type: "text", nullable: false),
                    CandidateRisks = table.Column<string>(type: "text", nullable: false),
                    AnalysisSummary = table.Column<string>(type: "text", nullable: false),
                    MatchScoreEstimation = table.Column<int>(type: "integer", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "double precision", nullable: false),
                    ReasoningSummary = table.Column<string>(type: "text", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_JobOffers_UserId",
                table: "JobOffers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_KeycloakId",
                table: "Users",
                column: "KeycloakId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobOfferAnalyses");

            migrationBuilder.DropTable(
                name: "JobOffers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
