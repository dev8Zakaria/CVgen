using AiCv.Api.Modules.Ai;
using AiCv.Api.Modules.Ai.DTOs;

namespace AiCv.Api.Tests.Infrastructure;

public sealed class FakeAiService : IAiService
{
    public AiJobAnalysisResponseDto Response { get; set; } = new()
    {
        ExtractedKeywords = ["FastAPI", "Docker"],
        SuggestedSkills = ["C#", "Python"],
        MatchScoreEstimation = 85,
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
            ExtractedKeywords = ["FastAPI", "Docker"],
            SuggestedSkills = ["C#", "Python"],
            MatchScoreEstimation = 85,
        };
    }
}
