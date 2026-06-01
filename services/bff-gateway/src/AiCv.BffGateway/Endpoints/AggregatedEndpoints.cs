namespace AiCv.BffGateway.Endpoints;

public static class AggregatedEndpoints
{
    public static void MapAggregatedEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api").RequireAuthorization("require-auth");

        // Point 8 : Bootstrap de l'application
        group.MapGet("/app/bootstrap", async (HttpContext context, IHttpClientFactory clientFactory, IConfiguration config) =>
        {
            var client = CreateAuthenticatedClient(context, clientFactory);
            var profileUrl = config["DownstreamServices:Profile"];
            var oppUrl = config["DownstreamServices:Opportunity"];
            var cvUrl = config["DownstreamServices:Cv"];

            // Appels parallèles
            var profileTask = client.GetFromJsonAsync<object>($"{profileUrl}/profile/me");
            var oppsTask = client.GetFromJsonAsync<object[]>($"{oppUrl}/opportunity");
            // var cvsTask = client.GetFromJsonAsync<object[]>($"{cvUrl}/cvs"); // A activer quand CV service sera prêt

            await Task.WhenAll(profileTask, oppsTask);

            var opps = await oppsTask ?? Array.Empty<object>();

            return Results.Ok(new
            {
                profile = await profileTask,
                opportunities = opps,
                cvs = new List<object>(),
                stats = new 
                {
                    profileCompleteness = 80, // A calculer si besoin
                    opportunitiesCount = opps.Length,
                    generatedCvsCount = 0
                }
            });
        });

        // Point 9 : Dashboard Summary
        group.MapGet("/dashboard/summary", async (HttpContext context, IHttpClientFactory clientFactory, IConfiguration config) =>
        {
            var client = CreateAuthenticatedClient(context, clientFactory);
            // Logique similaire au Bootstrap, mais on limite le retour aux stats et aux "recents"
            // (A implémenter de manière identique en filtrant les tableaux).
            return Results.Ok(new { message = "Dashboard Summary prêt à être branché" });
        });

        // Point 10 : Create and Analyze Opportunity (En 1 seul clic frontend)
        group.MapPost("/opportunity/analyze-new", async (HttpContext context, IHttpClientFactory clientFactory, IConfiguration config) =>
        {
            var client = CreateAuthenticatedClient(context, clientFactory);
            var oppUrl = config["DownstreamServices:Opportunity"];

            // 1. Lire le body venant du frontend
            var payload = await context.Request.ReadFromJsonAsync<object>();

            // 2. Créer l'opportunité
            var createResponse = await client.PostAsJsonAsync($"{oppUrl}/opportunity", payload);
            createResponse.EnsureSuccessStatusCode();
            // On suppose que la réponse contient l'ID (ex: { "id": "123", ... })
            var createdOpp = await createResponse.Content.ReadFromJsonAsync<dynamic>();
            string oppId = createdOpp?.id;

            // 3. Lancer l'analyse immédiatement
            var analyzeResponse = await client.PostAsync($"{oppUrl}/opportunity/{oppId}/analyze", null);
            analyzeResponse.EnsureSuccessStatusCode();

            // 4. Retourner l'opportunité complète
            var finalOpp = await client.GetFromJsonAsync<object>($"{oppUrl}/opportunity/{oppId}");
            return Results.Ok(finalOpp);
        });
    }

    // Point 15 : Health Checks
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health/downstream", async (IHttpClientFactory clientFactory, IConfiguration config) =>
        {
            var client = clientFactory.CreateClient();
            var status = new Dictionary<string, string>();

            async Task<string> CheckService(string url)
            {
                try {
                    var response = await client.GetAsync($"{url}/health");
                    return response.IsSuccessStatusCode ? "healthy" : "degraded";
                } catch { return "unavailable"; }
            }

            status["profile"] = await CheckService(config["DownstreamServices:Profile"]);
            status["opportunity"] = await CheckService(config["DownstreamServices:Opportunity"]);
            status["cv"] = await CheckService(config["DownstreamServices:Cv"]);

            return Results.Ok(new { status = status.ContainsValue("unavailable") ? "degraded" : "healthy", services = status });
        });
    }

    // Utilitaire pour transférer le token Keycloak (Point 13)
    private static HttpClient CreateAuthenticatedClient(HttpContext context, IHttpClientFactory factory)
    {
        var client = factory.CreateClient();
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(authHeader))
        {
            client.DefaultRequestHeaders.Add("Authorization", authHeader);
        }
        return client;
    }
}