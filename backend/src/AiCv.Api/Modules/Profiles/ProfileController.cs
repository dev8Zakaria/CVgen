using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AiCv.Api.Modules.Profiles.Services;
using System.Security.Claims;

namespace AiCv.Api.Modules.Profiles;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Bloque les requêtes sans token Keycloak valide
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        var fullName = User.FindFirst("name")?.Value ?? "";

        if (string.IsNullOrEmpty(keycloakId))
        {
            return Unauthorized("Token invalide ou ID manquant.");
        }

        // On récupère le profil (qui contient aussi l'objet User grâce à EF Core)
        var profile = await _profileService.GetOrCreateProfileAsync(keycloakId, email, fullName);

        // On mappe les entités vers notre DTO propre
        var response = new DTOs.ProfileResponseDto
        {
            Id = profile.Id,
            Email = profile.User?.Email ?? email,
            FullName = profile.User?.FullName ?? fullName,
            Role = profile.User?.Role ?? "User",
            Title = profile.Title,
            Summary = profile.Summary,
            Phone = profile.Phone,
            Location = profile.Location,
            UpdatedAt = profile.UpdatedAt
        };

        return Ok(response);
    }
}