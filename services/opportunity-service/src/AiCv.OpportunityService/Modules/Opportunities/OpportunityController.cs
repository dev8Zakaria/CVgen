using System.Security.Claims;
using AiCv.OpportunityService.Modules.Ai;
using AiCv.OpportunityService.Modules.Opportunities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCv.OpportunityService.Modules.Opportunities;

[ApiController]
[Route("[controller]")]
[Authorize]
public class OpportunityController : ControllerBase
{
    private readonly Services.OpportunityService _opportunityService;

    public OpportunityController(Services.OpportunityService opportunityService)
    {
        _opportunityService = opportunityService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOpportunityDto dto)
    {
        var keycloakId = GetSubject();
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Unauthorized("Invalid token or missing user.");
        }

        var result = await _opportunityService.CreateAsync(keycloakId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var keycloakId = GetSubject();
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Unauthorized("Invalid token or missing user.");
        }

        return Ok(await _opportunityService.GetAllByUserAsync(keycloakId));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var keycloakId = GetSubject();
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Unauthorized("Invalid token or missing user.");
        }

        var result = await _opportunityService.GetByIdAsync(id, keycloakId);
        return result is null ? NotFound($"Opportunity {id} not found.") : Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOpportunityDto dto)
    {
        var keycloakId = GetSubject();
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Unauthorized("Invalid token or missing user.");
        }

        var result = await _opportunityService.UpdateAsync(id, keycloakId, dto);
        return result is null ? NotFound($"Opportunity {id} not found.") : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var keycloakId = GetSubject();
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Unauthorized("Invalid token or missing user.");
        }

        var deleted = await _opportunityService.DeleteAsync(id, keycloakId);
        return deleted ? NoContent() : NotFound($"Opportunity {id} not found.");
    }

    [HttpPost("{id:guid}/analyze")]
    public async Task<IActionResult> Analyze(Guid id, CancellationToken cancellationToken)
    {
        var keycloakId = GetSubject();
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Unauthorized("Invalid token or missing user.");
        }

        try
        {
            var result = await _opportunityService.AnalyzeAsync(id, keycloakId, cancellationToken);
            return result is null ? NotFound($"Opportunity {id} not found.") : Ok(result);
        }
        catch (AiAnalysisFailedException exception)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = exception.Message
            });
        }
    }

    private string? GetSubject()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
    }
}
