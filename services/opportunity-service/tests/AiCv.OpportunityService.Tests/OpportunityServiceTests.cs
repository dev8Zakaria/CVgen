using AiCv.OpportunityService.Data;
using AiCv.OpportunityService.Modules.Ai;
using AiCv.OpportunityService.Modules.Ai.DTOs;
using AiCv.OpportunityService.Modules.Opportunities.DTOs;
using AiCv.OpportunityService.Modules.Opportunities.Repositories;
using Microsoft.EntityFrameworkCore;
using OpportunityDomainService = AiCv.OpportunityService.Modules.Opportunities.Services.OpportunityService;

namespace AiCv.OpportunityService.Tests;

public class OpportunityServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesOpportunityForKeycloakUser()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var opportunity = await service.CreateAsync("keycloak-user-1", new CreateOpportunityDto
        {
            Title = " Backend Developer ",
            CompanyName = " Test Company ",
            Description = "Build APIs"
        });

        Assert.Equal("Backend Developer", opportunity.Title);
        Assert.Equal("Test Company", opportunity.CompanyName);
        Assert.Equal("pending", opportunity.AnalysisStatus);
        Assert.Single(context.Users);
        Assert.Single(context.JobOffers);
    }

    [Fact]
    public async Task GetAllByUserAsync_ReturnsOnlyCurrentUsersOpportunities()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        await service.CreateAsync("keycloak-user-1", new CreateOpportunityDto { Title = "A", CompanyName = "C1" });
        await service.CreateAsync("keycloak-user-2", new CreateOpportunityDto { Title = "B", CompanyName = "C2" });

        var opportunities = await service.GetAllByUserAsync("keycloak-user-1");

        Assert.Single(opportunities);
        Assert.Equal("A", opportunities[0].Title);
    }

    [Fact]
    public async Task AnalyzeAsync_StoresAnalysisAndMarksOpportunityCompleted()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var opportunity = await service.CreateAsync("keycloak-user-1", new CreateOpportunityDto
        {
            Title = "Backend Developer",
            CompanyName = "Test Company",
            Description = "Need .NET and PostgreSQL"
        });

        var analyzed = await service.AnalyzeAsync(opportunity.Id, "keycloak-user-1");

        Assert.NotNull(analyzed);
        Assert.Equal("completed", analyzed.AnalysisStatus);
        Assert.NotNull(analyzed.Analysis);
        Assert.Contains(".NET", analyzed.Analysis.ExtractedSkills);
        Assert.Single(context.JobOfferAnalyses);
    }

    [Fact]
    public async Task UpdateAsync_ResetsExistingAnalysis()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var opportunity = await service.CreateAsync("keycloak-user-1", new CreateOpportunityDto
        {
            Title = "Backend Developer",
            CompanyName = "Test Company",
            Description = "Need .NET"
        });
        await service.AnalyzeAsync(opportunity.Id, "keycloak-user-1");

        var updated = await service.UpdateAsync(opportunity.Id, "keycloak-user-1", new UpdateOpportunityDto
        {
            Title = "Frontend Developer",
            CompanyName = "New Company",
            Description = "Need React"
        });

        Assert.NotNull(updated);
        Assert.Equal("pending", updated.AnalysisStatus);
        Assert.Null(updated.Analysis);
        Assert.Empty(context.JobOfferAnalyses);
    }

    [Fact]
    public async Task DeleteAsync_RemovesOpportunity()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var opportunity = await service.CreateAsync("keycloak-user-1", new CreateOpportunityDto
        {
            Title = "Backend Developer",
            CompanyName = "Test Company"
        });

        var deleted = await service.DeleteAsync(opportunity.Id, "keycloak-user-1");

        Assert.True(deleted);
        Assert.Empty(context.JobOffers);
    }

    private static OpportunityDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OpportunityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OpportunityDbContext(options);
    }

    private static OpportunityDomainService CreateService(OpportunityDbContext context)
    {
        return new OpportunityDomainService(new OpportunityRepository(context), new FakeAiService());
    }

    private sealed class FakeAiService : IAiService
    {
        public Task<AiJobAnalysisResponseDto> AnalyzeJobAsync(string jobDescription, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AiJobAnalysisResponseDto
            {
                AnalysisSummary = "Role analysis completed.",
                ExtractedKeywords = ["backend", "api"],
                SuggestedSkills = [".NET", "PostgreSQL"],
                ExtractedResponsibilities = ["Build APIs"],
                DetectedTechnologies = ["ASP.NET Core"],
                DetectedExperienceLevel = "mid",
                DetectedLocation = "remote",
                DetectedContractType = "full-time",
                MustHaveRequirements = [".NET"],
                NiceToHaveRequirements = ["Docker"],
                CvFocusPoints = ["API delivery"],
                CandidateRisks = ["Missing cloud details"],
                ReasoningSummary = "Matched backend requirements."
            });
        }
    }
}
