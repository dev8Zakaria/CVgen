using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCv.CvService.Data.Migrations
{
    public partial class AddCvGenerationJobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CvGenerationJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KeycloakId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneratedCvId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    AssetFileName = table.Column<string>(type: "text", nullable: true),
                    ProfileSnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    OpportunitySnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CvGenerationJobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CvGenerationJobs_GeneratedCvId",
                table: "CvGenerationJobs",
                column: "GeneratedCvId");

            migrationBuilder.CreateIndex(
                name: "IX_CvGenerationJobs_KeycloakId",
                table: "CvGenerationJobs",
                column: "KeycloakId");

            migrationBuilder.CreateIndex(
                name: "IX_CvGenerationJobs_Status",
                table: "CvGenerationJobs",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CvGenerationJobs");
        }
    }
}
