using System.Net;
using System.Net.Http.Json;
using AiCv.Api.Modules.Cvs.DTOs;
using AiCv.Api.Tests.Infrastructure;
using FluentAssertions;

namespace AiCv.Api.Tests.Cvs;

public sealed class CvEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CvEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.FakeAiService.Reset();
    }

    [Fact]
    public async Task Generate_WithAuthenticatedUserAndAnalyzedOpportunity_ReturnsPreviewPayload()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"cv-{Guid.NewGuid()}", email: "zakaria@example.com", name: "Zakaria Test");

        await client.GetAsync("/api/profile/me");

        await client.PutAsJsonAsync("/api/profile/me", new
        {
            title = "Backend Developer",
            summary = "Backend engineer focused on API design and distributed systems.",
            phone = "+212600000000",
            location = "Casablanca",
        });

        var createResponse = await client.PostAsJsonAsync("/api/opportunity", new
        {
            title = "Backend Developer",
            companyName = "Oracle",
            description = "We are looking for a backend developer with experience in C#, Docker, APIs, and microservices.",
        });

        var createdOpportunity = await createResponse.Content.ReadFromJsonAsync<AiCv.Api.Modules.Opportunities.DTOs.OpportunityResponseDto>();
        await client.PostAsync($"/api/opportunity/{createdOpportunity!.Id}/analyze", null);

        var response = await client.PostAsJsonAsync("/api/cvs/generate", new
        {
            opportunityId = createdOpportunity.Id,
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var generatedCv = await response.Content.ReadFromJsonAsync<GeneratedCvResponseDto>();
        generatedCv.Should().NotBeNull();
        generatedCv!.Profile.FullName.Should().Be("Zakaria Test");
        generatedCv.Profile.Headline.Should().Be("Backend Developer");
        generatedCv.Target.JobTitle.Should().Be("Backend Developer");
        generatedCv.Target.CompanyName.Should().Be("Oracle");
        generatedCv.HighlightedSkills.Should().Contain(["C#", "Python"]);
        generatedCv.MatchingKeywords.Should().Contain(["FastAPI", "Docker"]);
        generatedCv.ProfessionalSummary.Should().Contain("Backend engineer focused on API design and distributed systems.");
    }

    [Fact]
    public async Task Generate_WithUnauthenticatedUser_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/cvs/generate", new
        {
            opportunityId = Guid.NewGuid(),
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Generate_WithoutAnalyzedOpportunity_ReturnsBadRequest()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"cv-{Guid.NewGuid()}", email: "zakaria@example.com", name: "Zakaria Test");

        await client.GetAsync("/api/profile/me");

        var createResponse = await client.PostAsJsonAsync("/api/opportunity", new
        {
            title = "Backend Developer",
            companyName = "Oracle",
            description = "We are looking for a backend developer with experience in C#, Docker, APIs, and microservices.",
        });

        var createdOpportunity = await createResponse.Content.ReadFromJsonAsync<AiCv.Api.Modules.Opportunities.DTOs.OpportunityResponseDto>();

        var response = await client.PostAsJsonAsync("/api/cvs/generate", new
        {
            opportunityId = createdOpportunity!.Id,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
