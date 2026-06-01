using System.Text.Json;

namespace AiCv.BffGateway.Endpoints;

public static class AggregatedEndpoints
{
    public static void MapAggregatedEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api").RequireAuthorization("require-auth");

        group.MapGet("/app/bootstrap", async (HttpContext context, IHttpClientFactory clientFactory, IConfiguration config) =>
        {
            var client = CreateAuthenticatedClient(context, clientFactory);
            var profileUrl = RequireConfig(config, "DownstreamServices:Profile");
            var oppUrl = RequireConfig(config, "DownstreamServices:Opportunity");
            var cvUrl = RequireConfig(config, "DownstreamServices:Cv");

            var profileTask = GetJsonOrDefault<object?>(client, $"{profileUrl}/profile/me", null);
            var opportunitiesTask = GetJsonOrDefault<object[]>(client, $"{oppUrl}/opportunity", Array.Empty<object>());
            var cvsTask = GetJsonOrDefault<object[]>(client, $"{cvUrl}/cvs", Array.Empty<object>());

            await Task.WhenAll(profileTask, opportunitiesTask, cvsTask);

            var opportunities = await opportunitiesTask;
            var cvs = await cvsTask;

            return Results.Ok(new
            {
                profile = await profileTask,
                opportunities,
                cvs,
                stats = new
                {
                    profileCompleteness = 80,
                    opportunitiesCount = opportunities.Length,
                    generatedCvsCount = cvs.Length
                }
            });
        });

        group.MapGet("/dashboard/summary", async (HttpContext context, IHttpClientFactory clientFactory, IConfiguration config) =>
        {
            var client = CreateAuthenticatedClient(context, clientFactory);
            var profileUrl = RequireConfig(config, "DownstreamServices:Profile");
            var oppUrl = RequireConfig(config, "DownstreamServices:Opportunity");
            var cvUrl = RequireConfig(config, "DownstreamServices:Cv");

            var profileTask = GetJsonOrDefault<object?>(client, $"{profileUrl}/profile/me", null);
            var opportunitiesTask = GetJsonOrDefault<object[]>(client, $"{oppUrl}/opportunity", Array.Empty<object>());
            var cvsTask = GetJsonOrDefault<object[]>(client, $"{cvUrl}/cvs", Array.Empty<object>());

            await Task.WhenAll(profileTask, opportunitiesTask, cvsTask);

            var opportunities = await opportunitiesTask;
            var cvs = await cvsTask;
            var recentOpportunities = opportunities.Take(3).ToArray();
            var recentCvs = cvs.Take(3).ToArray();

            return Results.Ok(new
            {
                profile = await profileTask,
                stats = new
                {
                    generatedCvs = cvs.Length,
                    jobOffers = opportunities.Length,
                    currentFocus = TryReadString(recentOpportunities.FirstOrDefault(), "companyName") ?? "Profile"
                },
                recentCvs,
                recentOpportunities
            });
        });

        group.MapPost("/opportunity/analyze-new", async (HttpContext context, IHttpClientFactory clientFactory, IConfiguration config) =>
        {
            var client = CreateAuthenticatedClient(context, clientFactory);
            var oppUrl = RequireConfig(config, "DownstreamServices:Opportunity");
            var payload = await context.Request.ReadFromJsonAsync<object>();

            var createResponse = await client.PostAsJsonAsync($"{oppUrl}/opportunity", payload);
            if (!createResponse.IsSuccessStatusCode)
            {
                return Results.StatusCode((int)createResponse.StatusCode);
            }

            var createdOpportunity = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            if (!createdOpportunity.TryGetProperty("id", out var idProperty))
            {
                return Results.Problem(
                    title: "Opportunity service returned an invalid create response.",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            var opportunityId = idProperty.GetString();
            if (string.IsNullOrWhiteSpace(opportunityId))
            {
                return Results.Problem(
                    title: "Opportunity service returned an empty opportunity id.",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            var analyzeResponse = await client.PostAsync($"{oppUrl}/opportunity/{opportunityId}/analyze", null);
            if (!analyzeResponse.IsSuccessStatusCode)
            {
                return Results.StatusCode((int)analyzeResponse.StatusCode);
            }

            var finalOpportunity = await GetJsonOrDefault<object?>(client, $"{oppUrl}/opportunity/{opportunityId}", null);
            return Results.Ok(finalOpportunity);
        });
    }

    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health/downstream", async (IHttpClientFactory clientFactory, IConfiguration config) =>
        {
            var client = clientFactory.CreateClient();
            var services = new Dictionary<string, string>
            {
                ["profile"] = await CheckService(client, config["DownstreamServices:Profile"]),
                ["opportunity"] = await CheckService(client, config["DownstreamServices:Opportunity"]),
                ["cv"] = await CheckService(client, config["DownstreamServices:Cv"]),
                ["ai"] = await CheckService(client, config["DownstreamServices:Ai"])
            };

            var overallStatus = services.Values.Any(status => status is "unavailable" or "degraded")
                ? "degraded"
                : "healthy";

            return Results.Ok(new { status = overallStatus, services });
        })
        .AllowAnonymous();
    }

    private static HttpClient CreateAuthenticatedClient(HttpContext context, IHttpClientFactory factory)
    {
        var client = factory.CreateClient();
        var authHeader = context.Request.Headers.Authorization.ToString();

        if (!string.IsNullOrWhiteSpace(authHeader))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authHeader);
        }

        return client;
    }

    private static async Task<string> CheckService(HttpClient client, string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "not-configured";
        }

        try
        {
            using var response = await client.GetAsync($"{url}/health");
            return response.IsSuccessStatusCode ? "healthy" : "degraded";
        }
        catch
        {
            return "unavailable";
        }
    }

    private static async Task<T> GetJsonOrDefault<T>(HttpClient client, string url, T fallback)
    {
        try
        {
            using var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return fallback;
            }

            return await response.Content.ReadFromJsonAsync<T>() ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private static string RequireConfig(IConfiguration config, string key)
    {
        return config[key] ?? throw new InvalidOperationException($"Missing required configuration value '{key}'.");
    }

    private static string? TryReadString(object? value, string propertyName)
    {
        if (value is JsonElement element &&
            element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }

        return null;
    }
}
