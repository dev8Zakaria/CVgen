using System.Net;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AiCv.BffGateway.Tests;

public class BffEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BffEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IHttpClientFactory>();
                services.AddSingleton<IHttpClientFactory, FakeHttpClientFactory>();
            });
        });
    }

    [Fact]
    public async Task Health_ReturnsHealthyStatus()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("healthy", body);
        Assert.Contains("bff-gateway", body);
    }

    [Fact]
    public async Task Info_ReturnsBffMetadata()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/info");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("AI CV BFF Gateway", body);
        Assert.Contains("DownstreamServices", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DownstreamHealth_ReturnsConfiguredServices()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/downstream");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("profile", body);
        Assert.Contains("opportunity", body);
        Assert.Contains("cv", body);
        Assert.Contains("ai", body);
        Assert.Contains("healthy", body);
    }

    [Fact]
    public async Task DashboardSummary_RequiresAuthentication()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/dashboard/summary");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(new FakeHttpMessageHandler());
        }
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;

            if (path.Equals("/health", StringComparison.OrdinalIgnoreCase))
            {
                return JsonResponse("""{"status":"healthy"}""");
            }

            if (path.Equals("/profile/me", StringComparison.OrdinalIgnoreCase))
            {
                return JsonResponse("""{"fullName":"Test User","professionalTitle":"Developer"}""");
            }

            if (path.Equals("/opportunity", StringComparison.OrdinalIgnoreCase))
            {
                return JsonResponse("""[{"id":"opp-1","companyName":"Test Company"}]""");
            }

            if (path.Equals("/cvs", StringComparison.OrdinalIgnoreCase))
            {
                return JsonResponse("""[{"id":"cv-1","companyName":"Test Company"}]""");
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        private static Task<HttpResponseMessage> JsonResponse(string json)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }
}
