using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCv.CvService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCvServiceCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneratedCvs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KeycloakId = table.Column<string>(type: "text", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobTitle = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: false),
                    Template = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ProfessionalSummary = table.Column<string>(type: "text", nullable: false),
                    ContentJson = table.Column<string>(type: "jsonb", nullable: false),
                    AssetFileName = table.Column<string>(type: "text", nullable: true),
                    AssetContentType = table.Column<string>(type: "text", nullable: true),
                    AssetSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedCvs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedCvs_KeycloakId",
                table: "GeneratedCvs",
                column: "KeycloakId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedCvs_OpportunityId",
                table: "GeneratedCvs",
                column: "OpportunityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneratedCvs");
        }
    }
}
