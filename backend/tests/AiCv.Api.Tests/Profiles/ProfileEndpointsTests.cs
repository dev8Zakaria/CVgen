using System.Net;
using System.Net.Http.Json;
using AiCv.Api.Data;
using AiCv.Api.Modules.Profiles.DTOs;
using AiCv.Api.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AiCv.Api.Tests.Profiles;

public class ProfileEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProfileEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ── GET /api/profile/me ───────────────────────────────────────────────────

    [Fact]
    public async Task GetMyProfile_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/profile/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyProfile_WithAuthenticatedUser_AutoCreatesProfile()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"profile-{Guid.NewGuid()}", email: "zakaria@example.com", name: "Zakaria Test");

        var response = await client.GetAsync("/api/profile/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<ProfileResponseDto>();
        profile.Should().NotBeNull();
        profile!.Email.Should().Be("zakaria@example.com");
        profile.FullName.Should().Be("Zakaria Test");
        profile.Title.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetMyProfile_CalledTwice_DoesNotDuplicateUser()
    {
        var subject = $"profile-{Guid.NewGuid()}";
        var client = _factory.CreateClient()
            .WithTestAuth(subject: subject, email: "test@example.com", name: "Test");

        await client.GetAsync("/api/profile/me");
        await client.GetAsync("/api/profile/me");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var count = db.Users.Count(u => u.KeycloakId == subject);
        count.Should().Be(1);
    }

    // ── PUT /api/profile/me ───────────────────────────────────────────────────

    [Fact]
    public async Task PutProfile_WithValidData_Returns200()
    {
        var client = _factory.CreateClient()
            .WithTestAuth(subject: $"profile-{Guid.NewGuid()}");

        await client.GetAsync("/api/profile/me");

        var response = await client.PutAsJsonAsync("/api/profile/me", new
        {
            title = "Développeur Full Stack",
            summary = "Passionné par le web.",
            phone = "+212600000000",
            location = "Casablanca"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<ProfileResponseDto>();
        profile.Should().NotBeNull();
        profile!.Title.Should().Be("Développeur Full Stack");
        profile.Location.Should().Be("Casablanca");
    }

    [Fact]
    public async Task PutProfile_UpdatedFields_ArePersistedInDatabase()
    {
        var subject = $"profile-{Guid.NewGuid()}";
        var client = _factory.CreateClient().WithTestAuth(subject: subject);

        await client.GetAsync("/api/profile/me");

        await client.PutAsJsonAsync("/api/profile/me", new
        {
            title = "Lead Dev",
            summary = "Expert en architecture.",
            phone = "+212611111111",
            location = "Rabat"
        });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = db.Users.FirstOrDefault(u => u.KeycloakId == subject);
        user.Should().NotBeNull();
        var profile = db.Profiles.FirstOrDefault(p => p.UserId == user!.Id);
        profile.Should().NotBeNull();
        profile!.Title.Should().Be("Lead Dev");
        profile.Location.Should().Be("Rabat");
    }

    [Fact]
    public async Task PutProfile_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/profile/me", new
        {
            title = "Dev",
            summary = "",
            phone = "",
            location = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}