namespace AiCv.Api.Modules.Opportunities.DTOs;

public class OpportunityResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> ExtractedSkills { get; set; } = new();
    public List<string> ExtractedKeywords { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}