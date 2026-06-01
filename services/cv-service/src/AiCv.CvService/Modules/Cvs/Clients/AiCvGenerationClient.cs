using System.Net.Http.Json;
using AiCv.CvService.Modules.Cvs.DTOs;

namespace AiCv.CvService.Modules.Cvs.Clients;

public sealed class AiCvGenerationClient : IAiCvGenerationClient
{
    private readonly HttpClient _httpClient;

    public AiCvGenerationClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AiCvGenerationResponseDto> GenerateCvAsync(AiCvGenerationRequestDto request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/generate-cv", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<AiCvGenerationResponseDto>(cancellationToken: cancellationToken);
        return payload ?? throw new InvalidOperationException("AI service returned an empty CV generation response.");
    }
}
