using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AiCv.Api.Modules.Profiles.Services;
using AiCv.Api.Modules.Profiles.DTOs;

namespace AiCv.Api.Modules.Profiles;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Le Token Keycloak est obligatoire pour TOUTES les routes ici
public class ProfileController : ControllerBase
{
    private readonly ProfileService _profileService;

    public ProfileController(ProfileService profileService)
    {
        _profileService = profileService;
    }

    // GET: /api/profile/me
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        var fullName = User.FindFirst("name")?.Value ?? "";

        if (string.IsNullOrEmpty(keycloakId)) return Unauthorized();

        var response = await _profileService.GetOrCreateProfileAsync(keycloakId, email, fullName);
        return Ok(response);
    }

    // PUT: /api/profile/me
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] ProfileUpdateRequestDto request)
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(keycloakId)) return Unauthorized();

        var updatedProfile = await _profileService.UpdateProfileAsync(keycloakId, request);

        if (updatedProfile == null) return NotFound("Profil introuvable.");

        return Ok(updatedProfile);
    }

    // DELETE: /api/profile/me
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMyProfile()
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(keycloakId)) return Unauthorized();

        var success = await _profileService.DeleteProfileAsync(keycloakId);

        if (!success) return NotFound("Profil introuvable.");

        return NoContent(); // Code HTTP 204: Succès, mais rien à renvoyer
    }
}