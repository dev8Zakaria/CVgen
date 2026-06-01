using AiCv.OpportunityService.Modules.Ai.DTOs;

namespace AiCv.OpportunityService.Modules.Ai;

public interface IAiService
{
    Task<AiJobAnalysisResponseDto> AnalyzeJobAsync(string jobDescription, CancellationToken cancellationToken = default);
}
