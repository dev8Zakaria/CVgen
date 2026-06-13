using Microsoft.AspNetCore.Http;

namespace AiCv.Api.Modules.Cvs.DTOs;

public sealed class GenerateCvRequestDto
{
    public Guid OpportunityId { get; set; }
    public IFormFile? AssetFile { get; set; } // Pour MinIO
}

public sealed class GenerateCvJsonRequestDto
{
    public Guid OpportunityId { get; set; }
}

public sealed class GeneratedCvProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
}

public sealed class GeneratedCvTargetDto
{
    public Guid OpportunityId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}

public sealed class GeneratedCvResponseDto
{
    public Guid CvId { get; set; } // Identifiant de la base de données
    public string? AssetUrl { get; set; } // L'URL MinIO
    
    public GeneratedCvProfileDto Profile { get; set; } = new();
    public GeneratedCvTargetDto Target { get; set; } = new();
    public string ProfessionalSummary { get; set; } = string.Empty;
    
    public List<string> HighlightedSkills { get; set; } = [];
    public List<string> MatchingKeywords { get; set; } = [];
    public List<string> TailoredExperienceHints { get; set; } = [];
    public DateTime GeneratedAt { get; set; }
}
