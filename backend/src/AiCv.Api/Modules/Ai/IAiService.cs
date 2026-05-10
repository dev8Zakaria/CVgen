using AiCv.Api.Modules.Ai.DTOs;

namespace AiCv.Api.Modules.Ai;

public interface IAiService
{
    Task<AiJobAnalysisResponseDto> AnalyzeJobAsync(string jobDescription, CancellationToken cancellationToken = default);
}
