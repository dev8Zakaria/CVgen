using System.Security.Claims;
using AiCv.Api.Modules.Cvs.DTOs;
using AiCv.Api.Modules.Cvs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCv.Api.Modules.Cvs;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CvController : ControllerBase
{
    private readonly CvService _cvService;

    public CvController(CvService cvService)
    {
        _cvService = cvService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateCv([FromForm] GenerateCvRequestDto request)
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(keycloakId)) return Unauthorized();

        var response = await _cvService.GenerateInitialCvAsync(keycloakId, request);
        
        if (response == null) return BadRequest("Utilisateur introuvable. Veuillez créer un profil d'abord.");

        return Ok(response);
    }
}