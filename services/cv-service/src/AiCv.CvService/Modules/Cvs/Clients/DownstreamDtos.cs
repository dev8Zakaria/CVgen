using System.Text.Json.Serialization;

namespace AiCv.CvService.Modules.Cvs.Clients;

public sealed class ProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<ProfileExperienceDto> Experiences { get; set; } = [];
    public List<ProfileEducationDto> Educations { get; set; } = [];
    public List<ProfileProjectDto> Projects { get; set; } = [];
    public List<ProfileSkillDto> Skills { get; set; } = [];
    public List<ProfileLanguageDto> Languages { get; set; } = [];
    public List<ProfileCertificationDto> Certifications { get; set; } = [];
}

public sealed class ProfileExperienceDto
{
    public Guid Id { get; set; }
    public string Company { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public sealed class ProfileEducationDto
{
    public string School { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
}

public sealed class ProfileProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Technologies { get; set; } = string.Empty;
}

public sealed class ProfileSkillDto
{
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public sealed class ProfileLanguageDto
{
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
}

public sealed class ProfileCertificationDto
{
    public string Title { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
}

public sealed class OpportunityDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AnalysisStatus { get; set; } = string.Empty;
    public OpportunityAnalysisDto? Analysis { get; set; }
}

public sealed class OpportunityAnalysisDto
{
    public List<string> ExtractedSkills { get; set; } = [];
    public List<string> ExtractedKeywords { get; set; } = [];
    public List<string> ExtractedResponsibilities { get; set; } = [];
    public List<string> DetectedTechnologies { get; set; } = [];
    public string DetectedExperienceLevel { get; set; } = string.Empty;
    public string DetectedLocation { get; set; } = string.Empty;
    public string DetectedContractType { get; set; } = string.Empty;
    public List<string> MustHaveRequirements { get; set; } = [];
    public List<string> NiceToHaveRequirements { get; set; } = [];
    public List<string> CvFocusPoints { get; set; } = [];
    public List<string> CandidateRisks { get; set; } = [];
    public string AnalysisSummary { get; set; } = string.Empty;
}
