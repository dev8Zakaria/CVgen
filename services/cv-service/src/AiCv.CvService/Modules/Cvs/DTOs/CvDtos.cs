using System.Text.Json.Serialization;

namespace AiCv.CvService.Modules.Cvs.DTOs;

public sealed class GenerateCvRequestDto
{
    public Guid OpportunityId { get; set; }
    public IFormFile? AssetFile { get; set; }
}

public sealed class CvGenerationJobResponseDto
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid OpportunityId { get; set; }
    public Guid? GeneratedCvId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
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

public sealed class GeneratedCvContentDto
{
    public GeneratedCvHeaderDto Header { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public List<GeneratedCvExperienceDto> Experience { get; set; } = [];
    public List<GeneratedCvEducationDto> Education { get; set; } = [];
    public List<GeneratedCvProjectDto> Projects { get; set; } = [];
    public List<string> Skills { get; set; } = [];
    public List<GeneratedCvSkillGroupDto> SkillGroups { get; set; } = [];
    public List<GeneratedCvSimpleItemDto> Languages { get; set; } = [];
    public List<GeneratedCvSimpleItemDto> Certifications { get; set; } = [];
    public List<string> Notes { get; set; } = [];
    public GeneratedCvContentTargetDto Target { get; set; } = new();
}

public sealed class GeneratedCvHeaderDto
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public sealed class GeneratedCvExperienceDto
{
    public string Id { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<string> Bullets { get; set; } = [];
}

public sealed class GeneratedCvEducationDto
{
    public string School { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
}

public sealed class GeneratedCvProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Technologies { get; set; } = string.Empty;
    public List<string> Bullets { get; set; } = [];
}

public sealed class GeneratedCvSkillGroupDto
{
    public string Label { get; set; } = string.Empty;
    public List<string> Items { get; set; } = [];
}

public sealed class GeneratedCvSimpleItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
}

public sealed class GeneratedCvContentTargetDto
{
    public string Role { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
}

public sealed class GeneratedCvResponseDto
{
    public Guid Id { get; set; }
    public Guid CvId { get; set; }
    public string? AssetFileName { get; set; }
    public GeneratedCvProfileDto Profile { get; set; } = new();
    public GeneratedCvTargetDto Target { get; set; } = new();
    public string ProfessionalSummary { get; set; } = string.Empty;
    public List<string> HighlightedSkills { get; set; } = [];
    public List<string> MatchingKeywords { get; set; } = [];
    public List<string> TailoredExperienceHints { get; set; } = [];
    public GeneratedCvContentDto Content { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public sealed class GeneratedCvListItemDto
{
    public Guid Id { get; set; }
    public Guid OpportunityId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class AiCvGenerationRequestDto
{
    [JsonPropertyName("profile")]
    public AiCvProfileInputDto Profile { get; set; } = new();

    [JsonPropertyName("opportunity")]
    public AiCvOpportunityInputDto Opportunity { get; set; } = new();
}

public sealed class AiCvProfileInputDto
{
    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("headline")]
    public string Headline { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("skills")]
    public List<string> Skills { get; set; } = [];

    [JsonPropertyName("experiences")]
    public List<object> Experiences { get; set; } = [];

    [JsonPropertyName("educations")]
    public List<object> Educations { get; set; } = [];

    [JsonPropertyName("projects")]
    public List<object> Projects { get; set; } = [];

    [JsonPropertyName("languages")]
    public List<object> Languages { get; set; } = [];

    [JsonPropertyName("certifications")]
    public List<object> Certifications { get; set; } = [];
}

public sealed class AiCvOpportunityInputDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("company_name")]
    public string CompanyName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("analysis_summary")]
    public string AnalysisSummary { get; set; } = string.Empty;

    [JsonPropertyName("extracted_skills")]
    public List<string> ExtractedSkills { get; set; } = [];

    [JsonPropertyName("extracted_keywords")]
    public List<string> ExtractedKeywords { get; set; } = [];

    [JsonPropertyName("extracted_responsibilities")]
    public List<string> ExtractedResponsibilities { get; set; } = [];

    [JsonPropertyName("detected_technologies")]
    public List<string> DetectedTechnologies { get; set; } = [];

    [JsonPropertyName("detected_experience_level")]
    public string DetectedExperienceLevel { get; set; } = string.Empty;

    [JsonPropertyName("detected_location")]
    public string DetectedLocation { get; set; } = string.Empty;

    [JsonPropertyName("detected_contract_type")]
    public string DetectedContractType { get; set; } = string.Empty;

    [JsonPropertyName("must_have_requirements")]
    public List<string> MustHaveRequirements { get; set; } = [];

    [JsonPropertyName("nice_to_have_requirements")]
    public List<string> NiceToHaveRequirements { get; set; } = [];

    [JsonPropertyName("cv_focus_points")]
    public List<string> CvFocusPoints { get; set; } = [];

    [JsonPropertyName("candidate_risks")]
    public List<string> CandidateRisks { get; set; } = [];
}

public sealed class AiCvGenerationResponseDto
{
    [JsonPropertyName("professional_summary")]
    public string ProfessionalSummary { get; set; } = string.Empty;

    [JsonPropertyName("highlighted_skills")]
    public List<string> HighlightedSkills { get; set; } = [];

    [JsonPropertyName("matching_keywords")]
    public List<string> MatchingKeywords { get; set; } = [];

    [JsonPropertyName("tailored_experience_hints")]
    public List<string> TailoredExperienceHints { get; set; } = [];

    [JsonPropertyName("optimized_experiences")]
    public List<AiOptimizedExperienceDto> OptimizedExperiences { get; set; } = [];

    [JsonPropertyName("optimized_projects")]
    public List<AiOptimizedProjectDto> OptimizedProjects { get; set; } = [];
}

public sealed class AiOptimizedExperienceDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("bullets")]
    public List<string> Bullets { get; set; } = [];
}

public sealed class AiOptimizedProjectDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("bullets")]
    public List<string> Bullets { get; set; } = [];
}
