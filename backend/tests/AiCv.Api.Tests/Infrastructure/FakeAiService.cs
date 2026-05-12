using AiCv.Api.Modules.Ai;
using AiCv.Api.Modules.Ai.DTOs;

namespace AiCv.Api.Tests.Infrastructure;

public sealed class FakeAiService : IAiService
{
    public AiJobAnalysisResponseDto Response { get; set; } = new()
    {
        AnalysisSummary = "Backend-focused role emphasizing APIs, Docker, and distributed systems.",
        ExtractedKeywords = ["FastAPI", "Docker"],
        SuggestedSkills = ["C#", "Python"],
        ExtractedResponsibilities = ["Design and maintain backend services"],
        DetectedTechnologies = [".NET", "Docker"],
        DetectedExperienceLevel = "mid-senior",
        DetectedLocation = "Remote",
        DetectedContractType = "full-time",
        MustHaveRequirements = ["Strong C# and .NET experience"],
        NiceToHaveRequirements = ["Cloud deployment experience"],
        CvFocusPoints = ["Highlight backend API projects"],
        CandidateRisks = ["Role may expect deeper cloud production experience"],
        MatchScoreEstimation = 85,
        ConfidenceScore = 0.88,
        ReasoningSummary = "Strong alignment with backend APIs and containerized workflows.",
    };

    public Exception? ExceptionToThrow { get; set; }

    public Task<AiJobAnalysisResponseDto> AnalyzeJobAsync(string jobDescription, CancellationToken cancellationToken = default)
    {
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        return Task.FromResult(Response);
    }

    public void Reset()
    {
        ExceptionToThrow = null;
        Response = new AiJobAnalysisResponseDto
        {
            AnalysisSummary = "Backend-focused role emphasizing APIs, Docker, and distributed systems.",
            ExtractedKeywords = ["FastAPI", "Docker"],
            SuggestedSkills = ["C#", "Python"],
            ExtractedResponsibilities = ["Design and maintain backend services"],
            DetectedTechnologies = [".NET", "Docker"],
            DetectedExperienceLevel = "mid-senior",
            DetectedLocation = "Remote",
            DetectedContractType = "full-time",
            MustHaveRequirements = ["Strong C# and .NET experience"],
            NiceToHaveRequirements = ["Cloud deployment experience"],
            CvFocusPoints = ["Highlight backend API projects"],
            CandidateRisks = ["Role may expect deeper cloud production experience"],
            MatchScoreEstimation = 85,
            ConfidenceScore = 0.88,
            ReasoningSummary = "Strong alignment with backend APIs and containerized workflows.",
        };
    }
}
