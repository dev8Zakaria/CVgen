using System.Net;
using System.Net.Http.Json;
using AiCv.Api.Data;
using AiCv.Api.Modules.Opportunities.DTOs;
using AiCv.Api.Modules.Opportunities.Entities;
using AiCv.Api.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AiCv.Api.Tests.Opportunities;

public class OpportunityEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public OpportunityEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateOpportunity_WithAuthenticatedUser_CreatesRawJobOffer()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"opportunity-{Guid.NewGuid()}");

        await client.GetAsync("/api/profile/me");

        var createResponse = await client.PostAsJsonAsync("/api/opportunity", new
        {
            title = "Frontend Engineer",
            companyName = "OpenAI",
            description = "Build product experiences around AI.",
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<OpportunityResponseDto>();
        created.Should().NotBeNull();
        created!.Title.Should().Be("Frontend Engineer");
        created.CompanyName.Should().Be("OpenAI");
        created.AnalysisStatus.Should().Be("pending");
        created.Analysis.Should().BeNull();
    }

    [Fact]
    public async Task UpdateOpportunity_ResetsAnalysisStatusAndClearsStoredAnalysis()
    {
        var subject = $"opportunity-update-{Guid.NewGuid()}";
        var client = _factory.CreateClient().WithTestAuth(subject: subject);

        await client.GetAsync("/api/profile/me");

        var createResponse = await client.PostAsJsonAsync("/api/opportunity", new
        {
            title = "Backend Engineer",
            companyName = "Initial Corp",
            description = "Initial description",
        });
        var created = await createResponse.Content.ReadFromJsonAsync<OpportunityResponseDto>();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.JobOfferAnalyses.Add(new JobOfferAnalysis
            {
                JobOfferId = created!.Id,
                ExtractedSkills = ["C#", ".NET"],
                AnalysisSummary = "Old analysis",
            });

            var jobOffer = await dbContext.JobOffers.FindAsync(created.Id);
            jobOffer!.AnalysisStatus = "completed";
            await dbContext.SaveChangesAsync();
        }

        var updateResponse = await client.PutAsJsonAsync($"/api/opportunity/{created!.Id}", new
        {
            title = "Senior Backend Engineer",
            companyName = "Updated Corp",
            description = "Updated description",
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await updateResponse.Content.ReadFromJsonAsync<OpportunityResponseDto>();
        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Senior Backend Engineer");
        updated.CompanyName.Should().Be("Updated Corp");
        updated.AnalysisStatus.Should().Be("pending");
        updated.Analysis.Should().BeNull();
    }
}
