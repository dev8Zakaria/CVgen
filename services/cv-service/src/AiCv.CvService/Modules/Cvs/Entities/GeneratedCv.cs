using System.ComponentModel.DataAnnotations;

namespace AiCv.CvService.Modules.Cvs.Entities;

public class GeneratedCv
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string KeycloakId { get; set; } = string.Empty;

    public Guid OpportunityId { get; set; }

    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Template { get; set; } = "Atelier Ivory";
    public string Status { get; set; } = "Saved";
    public string ProfessionalSummary { get; set; } = string.Empty;
    public string ContentJson { get; set; } = "{}";
    public string? AssetFileName { get; set; }
    public string? AssetContentType { get; set; }
    public long? AssetSizeBytes { get; set; }
    public string? PdfObjectKey { get; set; }
    public string? PdfBucketName { get; set; }
    public long? PdfSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
