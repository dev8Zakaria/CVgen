using System.Text.Json.Serialization;

namespace AiCv.Api.Modules.Ai.DTOs;

public sealed class AiJobAnalysisRequestDto
{
    [JsonPropertyName("job_description")]
    public string JobDescription { get; set; } = string.Empty;
}

public sealed class AiJobAnalysisResponseDto
{
    [JsonPropertyName("extracted_keywords")]
    public List<string> ExtractedKeywords { get; set; } = [];

    [JsonPropertyName("suggested_skills")]
    public List<string> SuggestedSkills { get; set; } = [];

    [JsonPropertyName("match_score_estimation")]
    public int MatchScoreEstimation { get; set; }
}
