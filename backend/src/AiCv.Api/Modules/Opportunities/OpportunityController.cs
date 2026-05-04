using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AiCv.Api.Data;
using AiCv.Api.Modules.Opportunities.DTOs;
using AiCv.Api.Modules.Opportunities.Services;

namespace AiCv.Api.Modules.Opportunities;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OpportunityController : ControllerBase
{
    private readonly OpportunityService _opportunityService;
    private readonly AppDbContext _context;

    public OpportunityController(OpportunityService opportunityService, AppDbContext context)
    {
        _opportunityService = opportunityService;
        _context = context;
    }

    // POST: /api/opportunity
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOpportunityDto dto)
    {
        var userId = await ResolveUserIdAsync();
        if (userId is null) return Unauthorized("Token invalide ou utilisateur introuvable.");

        var result = await _opportunityService.CreateAsync(userId.Value, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // GET: /api/opportunity
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = await ResolveUserIdAsync();
        if (userId is null) return Unauthorized("Token invalide ou utilisateur introuvable.");

        var result = await _opportunityService.GetAllByUserAsync(userId.Value);
        return Ok(result);
    }

    // GET: /api/opportunity/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = await ResolveUserIdAsync();
        if (userId is null) return Unauthorized("Token invalide ou utilisateur introuvable.");

        var result = await _opportunityService.GetByIdAsync(id, userId.Value);
        if (result is null) return NotFound($"Opportunité {id} introuvable.");

        return Ok(result);
    }

    // Traduit le keycloakId du JWT vers le Guid interne de la table Users
    private async Task<Guid?> ResolveUserIdAsync()
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(keycloakId)) return null;

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);

        return user?.Id;
    }
}