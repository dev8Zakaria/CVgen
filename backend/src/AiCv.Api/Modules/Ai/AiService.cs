using System.Net.Http.Json;
using AiCv.Api.Modules.Ai.DTOs;

namespace AiCv.Api.Modules.Ai;

public sealed class AiService : IAiService
{
    private readonly HttpClient _httpClient;

    public AiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AiJobAnalysisResponseDto> AnalyzeJobAsync(string jobDescription, CancellationToken cancellationToken = default)
    {
        var request = new AiJobAnalysisRequestDto
        {
            JobDescription = jobDescription.Trim(),
        };

        using var response = await _httpClient.PostAsJsonAsync("/analyze-job", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new AiAnalysisFailedException($"AI service returned status code {(int)response.StatusCode}.");
        }

        var payload = await response.Content.ReadFromJsonAsync<AiJobAnalysisResponseDto>(cancellationToken: cancellationToken);
        if (payload is null)
        {
            throw new AiAnalysisFailedException("AI service returned an empty response.");
        }

        return payload;
    }
}
