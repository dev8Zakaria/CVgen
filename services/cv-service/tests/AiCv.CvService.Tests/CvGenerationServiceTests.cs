using System.Text;
using AiCv.CvService.Data;
using AiCv.CvService.Modules.Cvs.Clients;
using AiCv.CvService.Modules.Cvs.DTOs;
using AiCv.CvService.Modules.Cvs.Repositories;
using AiCv.CvService.Modules.Cvs.Services;
using AiCv.CvService.Modules.Cvs.Storage;
using Microsoft.EntityFrameworkCore;

namespace AiCv.CvService.Tests;

public class CvGenerationServiceTests
{
    [Fact]
    public async Task GenerateAsync_UsesProfileOpportunityAndAiContent()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var opportunityId = FakeOpportunityClient.OpportunityId;

        var generated = await service.GenerateAsync(
            "keycloak-user-1",
            "Bearer test-token",
            new GenerateCvRequestDto { OpportunityId = opportunityId });

        Assert.NotNull(generated);
        Assert.Equal("Backend Developer", generated.Target.JobTitle);
        Assert.Equal("Test Company", generated.Target.CompanyName);
        Assert.Contains(".NET", generated.HighlightedSkills);
        Assert.Contains("Tailor API delivery examples.", generated.TailoredExperienceHints);
        Assert.Contains("Built authenticated backend APIs aligned with platform integration requirements.", generated.Content.Experience[0].Bullets);
        Assert.Contains("Implemented CV generation workflows with service boundaries, API orchestration, and PostgreSQL-backed persistence.", generated.Content.Projects[0].Bullets);
        Assert.Single(context.GeneratedCvs);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyCurrentUsersCvs()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        await service.GenerateAsync("keycloak-user-1", "Bearer token", new GenerateCvRequestDto { OpportunityId = FakeOpportunityClient.OpportunityId });
        await service.GenerateAsync("keycloak-user-2", "Bearer token", new GenerateCvRequestDto { OpportunityId = FakeOpportunityClient.OpportunityId });

        var cvs = await service.GetAllAsync("keycloak-user-1");

        Assert.Single(cvs);
        Assert.Equal("Backend Developer", cvs[0].JobTitle);
    }

    [Fact]
    public async Task DeleteAsync_RemovesGeneratedCv()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var generated = await service.GenerateAsync("keycloak-user-1", "Bearer token", new GenerateCvRequestDto { OpportunityId = FakeOpportunityClient.OpportunityId });

        var deleted = await service.DeleteAsync(generated!.Id, "keycloak-user-1");

        Assert.True(deleted);
        Assert.Empty(context.GeneratedCvs);
    }

    [Fact]
    public async Task BuildDownloadPdfAsync_ReturnsPdfFile()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var generated = await service.GenerateAsync("keycloak-user-1", "Bearer token", new GenerateCvRequestDto { OpportunityId = FakeOpportunityClient.OpportunityId });

        var file = await service.BuildDownloadPdfAsync(generated!.Id, "keycloak-user-1");

        Assert.NotNull(file);
        Assert.EndsWith(".pdf", file.Value.FileName);
        Assert.StartsWith("%PDF", Encoding.ASCII.GetString(file.Value.Content[..4]));

        var pdfText = Encoding.ASCII.GetString(file.Value.Content);
        Assert.Contains("Test User", pdfText);
        Assert.Contains("Backend Developer", pdfText);
        Assert.Contains("PROFESSIONAL SUMMARY", pdfText);
        Assert.Contains("TECHNICAL SKILLS", pdfText);
        Assert.Contains("EDUCATION", pdfText);
        Assert.Contains("PROJECTS", pdfText);
        Assert.Contains("ATS Portfolio", pdfText);
        Assert.Contains("ASP.NET Core, React, PostgreSQL", pdfText);
        Assert.DoesNotContain("ROLE ALIGNMENT", pdfText);
        Assert.DoesNotContain("Tailor API delivery examples.", pdfText);
    }

    private static CvDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CvDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CvDbContext(options);
    }

    private static CvGenerationService CreateService(CvDbContext context)
    {
        return new CvGenerationService(
            new CvRepository(context),
            new FakeProfileClient(),
            new FakeOpportunityClient(),
            new FakeAiCvGenerationClient(),
            new FakeCvObjectStorage());
    }

    private sealed class FakeProfileClient : IProfileClient
    {
        public Task<ProfileDto?> GetCurrentProfileAsync(string authorizationHeader, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ProfileDto?>(new ProfileDto
            {
                FullName = "Test User",
                Email = "test@example.com",
                Phone = "+212600000000",
                Location = "Casablanca",
                Title = "Full Stack Developer",
                Summary = "Builds useful software.",
                Skills =
                [
                    new ProfileSkillDto { Name = ".NET" },
                    new ProfileSkillDto { Name = "PostgreSQL" }
                ],
                Experiences =
                [
                    new ProfileExperienceDto
                    {
                        Id = Guid.NewGuid(),
                        Company = "AI CV",
                        Position = "Developer",
                        StartDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        Description = "Built APIs. Improved reliability."
                    }
                ],
                Educations =
                [
                    new ProfileEducationDto
                    {
                        School = "Test Engineering School",
                        Degree = "Bachelor of Engineering",
                        Field = "Computer Science"
                    }
                ],
                Projects =
                [
                    new ProfileProjectDto
                    {
                        Name = "ATS Portfolio",
                        Description = "Built a portfolio and CV generation workflow.",
                        Technologies = "ASP.NET Core, React, PostgreSQL"
                    }
                ],
                Languages =
                [
                    new ProfileLanguageDto { Name = "English", Level = "Professional" }
                ],
                Certifications =
                [
                    new ProfileCertificationDto { Title = "Docker Fundamentals", Issuer = "Online Course", Year = "2025" }
                ]
            });
        }
    }

    private sealed class FakeOpportunityClient : IOpportunityClient
    {
        public static readonly Guid OpportunityId = Guid.NewGuid();

        public Task<OpportunityDto?> GetOpportunityAsync(Guid opportunityId, string authorizationHeader, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<OpportunityDto?>(new OpportunityDto
            {
                Id = opportunityId,
                Title = "Backend Developer",
                CompanyName = "Test Company",
                Description = "Need .NET and PostgreSQL.",
                Analysis = new OpportunityAnalysisDto
                {
                    AnalysisSummary = "Backend-heavy role.",
                    ExtractedSkills = [".NET", "PostgreSQL"],
                    ExtractedKeywords = ["API", "Docker"],
                    CvFocusPoints = ["API delivery"]
                }
            });
        }
    }

    private sealed class FakeAiCvGenerationClient : IAiCvGenerationClient
    {
        public Task<AiCvGenerationResponseDto> GenerateCvAsync(AiCvGenerationRequestDto request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AiCvGenerationResponseDto
            {
                ProfessionalSummary = "Full Stack Developer focused on backend APIs and PostgreSQL systems.",
                HighlightedSkills = [".NET", "PostgreSQL", "Docker"],
                MatchingKeywords = ["API", "Docker"],
                TailoredExperienceHints = ["Tailor API delivery examples."],
                OptimizedExperiences =
                [
                    new AiOptimizedExperienceDto
                    {
                        Id = request.Profile.Experiences[0].GetType().GetProperty("Id")?.GetValue(request.Profile.Experiences[0])?.ToString() ?? "",
                        Bullets =
                        [
                            "Built authenticated backend APIs aligned with platform integration requirements.",
                            "Improved delivery quality through automated tests, clear API contracts, and Docker-based local workflows.",
                            "Highlight API delivery examples."
                        ]
                    }
                ],
                OptimizedProjects =
                [
                    new AiOptimizedProjectDto
                    {
                        Name = "ATS Portfolio",
                        Bullets =
                        [
                            "Implemented CV generation workflows with service boundaries, API orchestration, and PostgreSQL-backed persistence.",
                            "Integrated React screens with backend endpoints to support profile, opportunity analysis, and CV download flows."
                        ]
                    }
                ]
            });
        }
    }

    private sealed class FakeCvObjectStorage : ICvObjectStorage
    {
        private readonly Dictionary<string, byte[]> _objects = new();

        public string BucketName => "generated-cvs-test";

        public Task UploadPdfAsync(string objectKey, byte[] content, CancellationToken cancellationToken = default)
        {
            _objects[objectKey] = content;
            return Task.CompletedTask;
        }

        public Task<byte[]?> GetPdfAsync(string objectKey, CancellationToken cancellationToken = default)
        {
            _objects.TryGetValue(objectKey, out var content);
            return Task.FromResult(content);
        }
    }
}
