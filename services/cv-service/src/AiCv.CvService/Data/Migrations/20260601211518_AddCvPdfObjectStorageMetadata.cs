using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCv.CvService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCvPdfObjectStorageMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PdfBucketName",
                table: "GeneratedCvs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PdfObjectKey",
                table: "GeneratedCvs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PdfSizeBytes",
                table: "GeneratedCvs",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfBucketName",
                table: "GeneratedCvs");

            migrationBuilder.DropColumn(
                name: "PdfObjectKey",
                table: "GeneratedCvs");

            migrationBuilder.DropColumn(
                name: "PdfSizeBytes",
                table: "GeneratedCvs");
        }
    }
}
