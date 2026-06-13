using System.Security.Claims;
using AiCv.Api.Modules.Cvs.DTOs;
using AiCv.Api.Modules.Cvs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCv.Api.Modules.Cvs;

[ApiController]
[Route("api")]
[Authorize]
public class CvController : ControllerBase
{
    private readonly CvService _cvService;

    public CvController(CvService cvService)
    {
        _cvService = cvService;
    }

    [HttpPost("cv/generate")]
    [HttpPost("cvs/generate")]
    [Consumes("multipart/form-data", "application/x-www-form-urlencoded")]
    public Task<IActionResult> GenerateCvFromForm([FromForm] GenerateCvRequestDto request)
    {
        return GenerateCv(request);
    }

    [HttpPost("cv/generate")]
    [HttpPost("cvs/generate")]
    [Consumes("application/json")]
    public Task<IActionResult> GenerateCvFromJson([FromBody] GenerateCvJsonRequestDto request)
    {
        return GenerateCv(new GenerateCvRequestDto
        {
            OpportunityId = request.OpportunityId,
        });
    }

    private async Task<IActionResult> GenerateCv(GenerateCvRequestDto request)
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(keycloakId)) return Unauthorized();

        var response = await _cvService.GenerateInitialCvAsync(keycloakId, request);

        if (response == null) return BadRequest("Unable to generate a CV for this opportunity.");

        return Ok(response);
    }
}
