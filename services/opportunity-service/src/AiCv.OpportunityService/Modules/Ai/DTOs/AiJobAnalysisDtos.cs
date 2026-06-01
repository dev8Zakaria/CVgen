using System.Text.Json.Serialization;

namespace AiCv.OpportunityService.Modules.Ai.DTOs;

public sealed class AiJobAnalysisRequestDto
{
    [JsonPropertyName("job_description")]
    public string JobDescription { get; set; } = string.Empty;
}

public sealed class AiJobAnalysisResponseDto
{
    [JsonPropertyName("analysis_summary")]
    public string AnalysisSummary { get; set; } = string.Empty;

    [JsonPropertyName("extracted_keywords")]
    public List<string> ExtractedKeywords { get; set; } = [];

    [JsonPropertyName("suggested_skills")]
    public List<string> SuggestedSkills { get; set; } = [];

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

    [JsonPropertyName("reasoning_summary")]
    public string ReasoningSummary { get; set; } = string.Empty;
}
