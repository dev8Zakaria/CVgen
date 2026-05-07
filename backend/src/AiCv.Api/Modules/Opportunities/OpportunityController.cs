using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AiCv.Api.Modules.Opportunities.DTOs;
using AiCv.Api.Modules.Opportunities.Services;

namespace AiCv.Api.Modules.Opportunities;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OpportunityController : ControllerBase
{
    private readonly OpportunityService _opportunityService;

    public OpportunityController(OpportunityService opportunityService)
    {
        _opportunityService = opportunityService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOpportunityDto dto)
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(keycloakId))
        {
            return Unauthorized("Token invalide ou utilisateur introuvable.");
        }

        var result = await _opportunityService.CreateAsync(keycloakId, dto);
        if (result is null)
        {
            return Unauthorized("Token invalide ou utilisateur introuvable.");
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(keycloakId))
        {
            return Unauthorized("Token invalide ou utilisateur introuvable.");
        }

        var result = await _opportunityService.GetAllByUserAsync(keycloakId);
        if (result is null)
        {
            return Unauthorized("Token invalide ou utilisateur introuvable.");
        }

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(keycloakId))
        {
            return Unauthorized("Token invalide ou utilisateur introuvable.");
        }

        var result = await _opportunityService.GetByIdAsync(id, keycloakId);
        if (result is null)
        {
            return NotFound($"Opportunité {id} introuvable.");
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOpportunityDto dto)
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(keycloakId))
        {
            return Unauthorized("Token invalide ou utilisateur introuvable.");
        }

        var result = await _opportunityService.UpdateAsync(id, keycloakId, dto);
        if (result is null)
        {
            return NotFound($"OpportunitÃ© {id} introuvable.");
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(keycloakId))
        {
            return Unauthorized("Token invalide ou utilisateur introuvable.");
        }

        var deleted = await _opportunityService.DeleteAsync(id, keycloakId);
        if (!deleted)
        {
            return NotFound($"OpportunitÃ© {id} introuvable.");
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/analyze")]
    public async Task<IActionResult> Analyze(Guid id, CancellationToken cancellationToken)
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(keycloakId))
        {
            return Unauthorized("Token invalide ou utilisateur introuvable.");
        }

        try
        {
            var result = await _opportunityService.AnalyzeAsync(id, keycloakId, cancellationToken);
            if (result is null)
            {
                return NotFound($"Opportunité {id} introuvable.");
            }

            return Ok(result);
        }
        catch (AiCv.Api.Modules.Ai.AiAnalysisFailedException exception)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = exception.Message,
            });
        }
    }
}
