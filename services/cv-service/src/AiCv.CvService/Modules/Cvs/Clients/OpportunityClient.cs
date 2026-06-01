using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AiCv.CvService.Modules.Cvs.Clients;

public sealed class OpportunityClient : IOpportunityClient
{
    private readonly HttpClient _httpClient;

    public OpportunityClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OpportunityDto?> GetOpportunityAsync(Guid opportunityId, string authorizationHeader, CancellationToken cancellationToken = default)
    {
        SetAuthorization(authorizationHeader);
        using var response = await _httpClient.GetAsync($"/opportunity/{opportunityId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<OpportunityDto>(cancellationToken: cancellationToken);
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
