using AiCv.ProfileService.Data;
using AiCv.ProfileService.Modules.Profiles.DTOs;
using AiCv.ProfileService.Modules.Profiles.Repositories;
using Microsoft.EntityFrameworkCore;
using ProfileDomainService = AiCv.ProfileService.Modules.Profiles.Services.ProfileService;

namespace AiCv.ProfileService.Tests;

public class ProfileServiceTests
{
    [Fact]
    public async Task GetOrCreateProfileAsync_CreatesProfileForNewKeycloakUser()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var profile = await service.GetOrCreateProfileAsync("keycloak-user-1", "user@test.com", "Test User");

        Assert.Equal("user@test.com", profile.Email);
        Assert.Equal("Test User", profile.FullName);
        Assert.Equal("Nouveau Profil", profile.Title);
        Assert.Single(context.Users);
        Assert.Single(context.Profiles);
    }

    [Fact]
    public async Task UpdateProfileAsync_ReplacesCollectionsAndReturnsUpdatedProfile()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        await service.GetOrCreateProfileAsync("keycloak-user-2", "user@test.com", "Test User");

        var updated = await service.UpdateProfileAsync("keycloak-user-2", new ProfileUpdateDto
        {
            Title = "Full Stack Developer",
            Summary = "Builds useful systems.",
            Phone = "+212600000000",
            Location = "Casablanca",
            Skills =
            [
                new SkillUpdateDto
                {
                    Name = "ASP.NET Core",
                    Level = "Advanced",
                    Category = "Backend"
                }
            ],
            Experiences =
            [
                new ExperienceUpdateDto
                {
                    Company = "AI CV",
                    Position = "Developer",
                    StartDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Description = "Built profile service."
                }
            ]
        });

        Assert.NotNull(updated);
        Assert.Equal("Full Stack Developer", updated.Title);
        Assert.Equal("Casablanca", updated.Location);
        Assert.Single(updated.Skills);
        Assert.Equal("ASP.NET Core", updated.Skills[0].Name);
        Assert.Single(updated.Experiences);
        Assert.Equal("AI CV", updated.Experiences[0].Company);
    }

    [Fact]
    public async Task DeleteProfileAsync_RemovesUserAndProfile()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        await service.GetOrCreateProfileAsync("keycloak-user-3", "user@test.com", "Test User");

        var deleted = await service.DeleteProfileAsync("keycloak-user-3");

        Assert.True(deleted);
        Assert.Empty(context.Users);
        Assert.Empty(context.Profiles);
    }

    private static ProfileDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ProfileDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ProfileDbContext(options);
    }

    private static ProfileDomainService CreateService(ProfileDbContext context)
    {
        return new ProfileDomainService(new ProfileRepository(context));
    }
}
