namespace AiCv.OpportunityService.Modules.Opportunities.DTOs;

public class OpportunityListResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string AnalysisStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OpportunityAnalysisDto
{
    public Guid Id { get; set; }
    public Guid JobOfferId { get; set; }
    public List<string> ExtractedSkills { get; set; } = [];
    public List<string> ExtractedKeywords { get; set; } = [];
    public List<string> ExtractedResponsibilities { get; set; } = [];
    public string DetectedExperienceLevel { get; set; } = string.Empty;
    public string DetectedLocation { get; set; } = string.Empty;
    public string DetectedContractType { get; set; } = string.Empty;
    public List<string> DetectedTechnologies { get; set; } = [];
    public List<string> MustHaveRequirements { get; set; } = [];
    public List<string> NiceToHaveRequirements { get; set; } = [];
    public List<string> CvFocusPoints { get; set; } = [];
    public List<string> CandidateRisks { get; set; } = [];
    public string AnalysisSummary { get; set; } = string.Empty;
    public string ReasoningSummary { get; set; } = string.Empty;
    public string RawAnalysisJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class OpportunityDetailsResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AnalysisStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public OpportunityAnalysisDto? Analysis { get; set; }
}
