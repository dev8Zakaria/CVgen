namespace AiCv.CvService.Modules.Cvs.Entities;

public class CvGenerationJob
{
    public Guid Id { get; set; }
    public string KeycloakId { get; set; } = string.Empty;
    public Guid OpportunityId { get; set; }
    public Guid? GeneratedCvId { get; set; }
    public string Status { get; set; } = "queued";
    public string? ErrorMessage { get; set; }
    public string? AssetFileName { get; set; }
    public string ProfileSnapshotJson { get; set; } = "{}";
    public string OpportunitySnapshotJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
