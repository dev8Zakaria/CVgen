namespace AiCv.Api.Modules.Opportunities.DTOs;

// Utilisé par GET /api/opportunities — liste légère
public class OpportunityListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Utilisé par GET /api/opportunities/{id} et POST — détail complet
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