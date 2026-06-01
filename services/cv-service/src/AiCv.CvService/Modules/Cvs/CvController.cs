using System.Security.Claims;
using AiCv.CvService.Modules.Cvs.DTOs;
using AiCv.CvService.Modules.Cvs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCv.CvService.Modules.Cvs;

[ApiController]
[Authorize]
public class CvController : ControllerBase
{
    private readonly CvGenerationService _cvService;

    public CvController(CvGenerationService cvService)
    {
        _cvService = cvService;
    }

    [HttpPost("cv/generate")]
    public async Task<IActionResult> GenerateCv([FromForm] GenerateCvRequestDto request, CancellationToken cancellationToken)
    {
        var keycloakId = GetSubject();
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Unauthorized();
        }

        var response = await _cvService.GenerateAsync(
            keycloakId,
            Request.Headers.Authorization.ToString(),
            request,
            cancellationToken);

        return response is null
            ? BadRequest("Profile or opportunity could not be loaded.")
            : Ok(response);
    }

    [HttpGet("cvs")]
    public async Task<IActionResult> GetAll()
    {
        var keycloakId = GetSubject();
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Unauthorized();
        }

        return Ok(await _cvService.GetAllAsync(keycloakId));
    }

    [HttpGet("cvs/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var keycloakId = GetSubject();
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Unauthorized();
        }

        var response = await _cvService.GetByIdAsync(id, keycloakId);
        return response is null ? NotFound($"CV {id} not found.") : Ok(response);
    }

    [HttpGet("cvs/{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var keycloakId = GetSubject();
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Unauthorized();
        }

        var file = await _cvService.BuildDownloadPdfAsync(id, keycloakId);
        if (file is null)
        {
            return NotFound($"CV {id} not found.");
        }

        return File(file.Value.Content, "application/pdf", file.Value.FileName);
    }

    [HttpDelete("cvs/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var keycloakId = GetSubject();
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Unauthorized();
        }

        var deleted = await _cvService.DeleteAsync(id, keycloakId);
        return deleted ? NoContent() : NotFound($"CV {id} not found.");
    }

    private string? GetSubject()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
    }
}
