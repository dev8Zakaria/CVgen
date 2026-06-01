using System.Security.Claims;
using AiCv.ProfileService.Modules.Profiles.DTOs;
using AiCv.ProfileService.Modules.Profiles.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCv.ProfileService.Modules.Profiles;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly Services.ProfileService _profileService;

    public ProfileController(Services.ProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var keycloakId = GetSubject();
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Unauthorized();
        }

        var response = await _profileService.GetOrCreateProfileAsync(
            keycloakId,
            User.FindFirst(ClaimTypes.Email)?.Value ?? "",
            User.FindFirst("name")?.Value ?? "");

        return Ok(response);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] ProfileUpdateDto request)
    {
        var keycloakId = GetSubject();
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Unauthorized();
        }

        var updatedProfile = await _profileService.UpdateProfileAsync(keycloakId, request);
        return updatedProfile == null ? NotFound("Profile not found.") : Ok(updatedProfile);
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMyProfile()
    {
        var keycloakId = GetSubject();
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Unauthorized();
        }

        var success = await _profileService.DeleteProfileAsync(keycloakId);
        return success ? NoContent() : NotFound("Profile not found.");
    }

    private string? GetSubject()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
    }
}
