using System.Net;
using System.Net.Http.Json;
using AiCv.Api.Data;
using AiCv.Api.Modules.Ai;
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
        _factory.FakeAiService.Reset();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task InitUserAsync(HttpClient client)
    {
        await client.GetAsync("/api/profile/me");
    }

    private async Task<OpportunityDetailsResponseDto> CreateOpportunityAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/opportunity", new
        {
            title = "Frontend Engineer",
            companyName = "OpenAI",
            description = "Build product experiences around AI.",
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<OpportunityDetailsResponseDto>();
        created.Should().NotBeNull();
        return created!;
    }

    // ── POST /api/opportunity ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateOpportunity_WithAuthenticatedUser_CreatesRawJobOffer()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"opportunity-{Guid.NewGuid()}");

        await InitUserAsync(client);

        var createResponse = await client.PostAsJsonAsync("/api/opportunity", new
        {
            title = "Frontend Engineer",
            companyName = "OpenAI",
            description = "Build product experiences around AI.",
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<OpportunityDetailsResponseDto>();
        created.Should().NotBeNull();
        created!.Title.Should().Be("Frontend Engineer");
        created.CompanyName.Should().Be("OpenAI");
        created.AnalysisStatus.Should().Be("pending");
        created.Analysis.Should().BeNull();
    }

    [Fact]
    public async Task CreateOpportunity_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/opportunity", new
        {
            title = "Frontend Engineer",
            companyName = "OpenAI",
            description = "Some description.",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOpportunity_WithMissingRequiredFields_Returns400()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"opportunity-{Guid.NewGuid()}");

        await InitUserAsync(client);

        // title manquant
        var response = await client.PostAsJsonAsync("/api/opportunity", new
        {
            companyName = "OpenAI",
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET /api/opportunity ──────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_WithNoOpportunities_ReturnsEmptyList()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"opportunity-{Guid.NewGuid()}");

        await InitUserAsync(client);

        var response = await client.GetAsync("/api/opportunity");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<OpportunityListResponseDto>>();
        list.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyTitleCompanyAndStatus()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"opportunity-{Guid.NewGuid()}");

        await InitUserAsync(client);
        await CreateOpportunityAsync(client);

        var response = await client.GetAsync("/api/opportunity");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<OpportunityListResponseDto>>();
        list.Should().NotBeNull().And.HaveCount(1);

        var item = list![0];
        item.Title.Should().Be("Frontend Engineer");
        item.CompanyName.Should().Be("OpenAI");
        item.AnalysisStatus.Should().Be("pending");
    }

    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/opportunity");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyCurrentUserOpportunities()
    {
        var subjectA = $"opportunity-a-{Guid.NewGuid()}";
        var subjectB = $"opportunity-b-{Guid.NewGuid()}";

        var clientA = _factory.CreateClient().WithTestAuth(subject: subjectA);
        var clientB = _factory.CreateClient().WithTestAuth(subject: subjectB);

        await InitUserAsync(clientA);
        await InitUserAsync(clientB);

        // A crée une opportunité
        await CreateOpportunityAsync(clientA);

        // B liste ses opportunités → doit être vide
        var response = await clientB.GetAsync("/api/opportunity");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<OpportunityListResponseDto>>();
        list.Should().NotBeNull().And.BeEmpty();
    }

    // ── GET /api/opportunity/{id} ─────────────────────────────────────────────

    [Fact]
    public async Task GetById_WithValidId_Returns200WithFullDetails()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"opportunity-{Guid.NewGuid()}");

        await InitUserAsync(client);
        var created = await CreateOpportunityAsync(client);

        var response = await client.GetAsync($"/api/opportunity/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await response.Content.ReadFromJsonAsync<OpportunityDetailsResponseDto>();
        detail.Should().NotBeNull();
        detail!.Id.Should().Be(created.Id);
        detail.Title.Should().Be("Frontend Engineer");
        detail.CompanyName.Should().Be("OpenAI");
        detail.Description.Should().Be("Build product experiences around AI.");
        detail.AnalysisStatus.Should().Be("pending");
        detail.Analysis.Should().BeNull();
    }

    [Fact]
    public async Task GetById_WithInvalidId_Returns404()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"opportunity-{Guid.NewGuid()}");

        await InitUserAsync(client);

        var response = await client.GetAsync($"/api/opportunity/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/opportunity/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_UserBCannotAccessUserAOpportunity()
    {
        var clientA = _factory.CreateClient().WithTestAuth(subject: $"opp-a-{Guid.NewGuid()}");
        var clientB = _factory.CreateClient().WithTestAuth(subject: $"opp-b-{Guid.NewGuid()}");

        await InitUserAsync(clientA);
        await InitUserAsync(clientB);

        var created = await CreateOpportunityAsync(clientA);

        // B tente d'accéder à l'opportunité de A
        var response = await clientB.GetAsync($"/api/opportunity/{created.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /api/opportunity/{id} ─────────────────────────────────────────────

    [Fact]
    public async Task UpdateOpportunity_ResetsAnalysisStatusAndClearsStoredAnalysis()
    {
        var subject = $"opportunity-update-{Guid.NewGuid()}";
        var client = _factory.CreateClient().WithTestAuth(subject: subject);

        await InitUserAsync(client);
        var created = await CreateOpportunityAsync(client);

        // Injecter manuellement une analyse en base
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.JobOfferAnalyses.Add(new JobOfferAnalysis
            {
                JobOfferId = created.Id,
                ExtractedSkills = ["C#", ".NET"],
                AnalysisSummary = "Old analysis",
            });
            var jobOffer = await db.JobOffers.FindAsync(created.Id);
            jobOffer!.AnalysisStatus = "completed";
            await db.SaveChangesAsync();
        }

        var updateResponse = await client.PutAsJsonAsync($"/api/opportunity/{created.Id}", new
        {
            title = "Senior Backend Engineer",
            companyName = "Updated Corp",
            description = "Updated description.",
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await updateResponse.Content.ReadFromJsonAsync<OpportunityDetailsResponseDto>();
        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Senior Backend Engineer");
        updated.CompanyName.Should().Be("Updated Corp");
        updated.AnalysisStatus.Should().Be("pending");
        updated.Analysis.Should().BeNull();
    }

    [Fact]
    public async Task UpdateOpportunity_WithInvalidId_Returns404()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"opportunity-{Guid.NewGuid()}");

        await InitUserAsync(client);

        var response = await client.PutAsJsonAsync($"/api/opportunity/{Guid.NewGuid()}", new
        {
            title = "New Title",
            companyName = "New Corp",
            description = "New description.",
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOpportunity_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/opportunity/{Guid.NewGuid()}", new
        {
            title = "New Title",
            companyName = "New Corp",
            description = "New description.",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateOpportunity_UserBCannotUpdateUserAOpportunity()
    {
        var clientA = _factory.CreateClient().WithTestAuth(subject: $"opp-a-{Guid.NewGuid()}");
        var clientB = _factory.CreateClient().WithTestAuth(subject: $"opp-b-{Guid.NewGuid()}");

        await InitUserAsync(clientA);
        await InitUserAsync(clientB);

        var created = await CreateOpportunityAsync(clientA);

        var response = await clientB.PutAsJsonAsync($"/api/opportunity/{created.Id}", new
        {
            title = "Hacked Title",
            companyName = "Hacked Corp",
            description = "Hacked.",
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── DELETE /api/opportunity/{id} ──────────────────────────────────────────

    [Fact]
    public async Task Delete_WithValidId_Returns204()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"opportunity-{Guid.NewGuid()}");

        await InitUserAsync(client);
        var created = await CreateOpportunityAsync(client);

        var response = await client.DeleteAsync($"/api/opportunity/{created.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_OpportunityIsRemovedFromDatabase()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"opportunity-{Guid.NewGuid()}");

        await InitUserAsync(client);
        var created = await CreateOpportunityAsync(client);

        await client.DeleteAsync($"/api/opportunity/{created.Id}");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jobOffer = await db.JobOffers.FindAsync(created.Id);
        jobOffer.Should().BeNull();
    }

    [Fact]
    public async Task Delete_LinkedAnalysisIsAlsoRemoved()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"opportunity-{Guid.NewGuid()}");

        await InitUserAsync(client);
        var created = await CreateOpportunityAsync(client);

        // Ajouter une analyse liée
        Guid analysisId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var analysis = new JobOfferAnalysis
            {
                JobOfferId = created.Id,
                ExtractedSkills = ["React"],
                AnalysisSummary = "Test analysis",
            };
            db.JobOfferAnalyses.Add(analysis);
            await db.SaveChangesAsync();
            analysisId = analysis.Id;
        }

        await client.DeleteAsync($"/api/opportunity/{created.Id}");

        // Vérifier que l'analyse est aussi supprimée (cascade)
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var deletedAnalysis = await verifyDb.JobOfferAnalyses.FindAsync(analysisId);
        deletedAnalysis.Should().BeNull();
    }

    [Fact]
    public async Task Delete_WithInvalidId_Returns404()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"opportunity-{Guid.NewGuid()}");

        await InitUserAsync(client);

        var response = await client.DeleteAsync($"/api/opportunity/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.DeleteAsync($"/api/opportunity/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_UserBCannotDeleteUserAOpportunity()
    {
        var clientA = _factory.CreateClient().WithTestAuth(subject: $"opp-a-{Guid.NewGuid()}");
        var clientB = _factory.CreateClient().WithTestAuth(subject: $"opp-b-{Guid.NewGuid()}");

        await InitUserAsync(clientA);
        await InitUserAsync(clientB);

        var created = await CreateOpportunityAsync(clientA);

        var response = await clientB.DeleteAsync($"/api/opportunity/{created.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Analyze_WithValidId_PersistsAnalysisAndReturnsCompletedStatus()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"opportunity-{Guid.NewGuid()}");

        await InitUserAsync(client);
        var created = await CreateOpportunityAsync(client);

        var response = await client.PostAsync($"/api/opportunity/{created.Id}/analyze", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var analyzed = await response.Content.ReadFromJsonAsync<OpportunityDetailsResponseDto>();
        analyzed.Should().NotBeNull();
        analyzed!.AnalysisStatus.Should().Be("completed");
        analyzed.Analysis.Should().NotBeNull();
        analyzed.Analysis!.ExtractedKeywords.Should().Contain(["FastAPI", "Docker"]);
        analyzed.Analysis.ExtractedSkills.Should().Contain(["C#", "Python"]);
        analyzed.Analysis.AnalysisSummary.Should().Be("Estimated match score: 85%");
    }

    [Fact]
    public async Task Analyze_WhenAiFails_MarksOpportunityAsFailed()
    {
        _factory.FakeAiService.ExceptionToThrow = new AiAnalysisFailedException("Mock AI failure.");

        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"opportunity-{Guid.NewGuid()}");

        await InitUserAsync(client);
        var created = await CreateOpportunityAsync(client);

        var response = await client.PostAsync($"/api/opportunity/{created.Id}/analyze", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jobOffer = await db.JobOffers.FindAsync(created.Id);
        jobOffer.Should().NotBeNull();
        jobOffer!.AnalysisStatus.Should().Be("failed");
    }

    [Fact]
    public async Task Analyze_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync($"/api/opportunity/{Guid.NewGuid()}/analyze", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
