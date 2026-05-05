using System.Net;
using System.Net.Http.Json;
using AiCv.Api.Modules.Profiles.DTOs;
using AiCv.Api.Tests.Infrastructure;
using FluentAssertions;

namespace AiCv.Api.Tests.Profiles;

public class ProfileEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProfileEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

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
}
