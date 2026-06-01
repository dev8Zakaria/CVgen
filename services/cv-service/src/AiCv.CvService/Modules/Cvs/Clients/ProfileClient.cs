using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AiCv.CvService.Modules.Cvs.Clients;

public sealed class ProfileClient : IProfileClient
{
    private readonly HttpClient _httpClient;

    public ProfileClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProfileDto?> GetCurrentProfileAsync(string authorizationHeader, CancellationToken cancellationToken = default)
    {
        SetAuthorization(authorizationHeader);
        using var response = await _httpClient.GetAsync("/profile/me", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ProfileDto>(cancellationToken: cancellationToken);
    }

    private void SetAuthorization(string authorizationHeader)
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            _httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authorizationHeader);
        }
    }
}
