using System.Security.Claims;
using AiCv.Api.Modules.Cvs.DTOs;
using AiCv.Api.Modules.Cvs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCv.Api.Modules.Cvs;

[ApiController]
[Route("api/cvs")]
[Authorize]
public sealed class CvController : ControllerBase
{
    private readonly CvService _cvService;

    public CvController(CvService cvService)
    {
        _cvService = cvService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateCvRequestDto request)
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Unauthorized("Token invalide ou utilisateur introuvable.");
        }

        var result = await _cvService.GenerateAsync(request.OpportunityId, keycloakId);
        if (result is null)
        {
            return BadRequest("The authenticated profile or analyzed job offer could not be used to generate a CV.");
        }

        return Ok(result);
    }
}
