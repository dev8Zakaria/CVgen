using AiCv.Api.Modules.Ai.DTOs;

namespace AiCv.Api.Modules.Ai;

public interface IAiService
{
    Task<AnalyzeJobResponseDto> AnalyzeJobAsync(string jobDescription, CancellationToken cancellationToken = default);
}
