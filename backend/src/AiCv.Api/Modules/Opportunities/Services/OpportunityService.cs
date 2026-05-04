using Microsoft.EntityFrameworkCore;
using AiCv.Api.Data;
using AiCv.Api.Modules.Opportunities.DTOs;
using AiCv.Api.Modules.Opportunities.Entities;
using AiCv.Api.Modules.Opportunities.Repositories;

namespace AiCv.Api.Modules.Opportunities.Services;

public class OpportunityService : IOpportunityService
{
    private readonly AppDbContext _context;

    public OpportunityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OpportunityResponseDto> CreateAsync(Guid userId, CreateOpportunityDto dto)
    {
        var opportunity = new Opportunity
        {
            UserId = userId,
            Title = dto.Title,
            Company = dto.Company,
            Description = dto.Description,
            ExtractedSkills = dto.ExtractedSkills,
            ExtractedKeywords = dto.ExtractedKeywords
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        return MapToDto(opportunity);
    }

    public async Task<List<OpportunityResponseDto>> GetAllByUserAsync(Guid userId)
    {
        var opportunities = await _context.Opportunities
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return opportunities.Select(MapToDto).ToList();
    }

    public async Task<OpportunityResponseDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var opportunity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        return opportunity is null ? null : MapToDto(opportunity);
    }

    private static OpportunityResponseDto MapToDto(Opportunity o) => new()
    {
        Id = o.Id,
        UserId = o.UserId,
        Title = o.Title,
        Company = o.Company,
        Description = o.Description,
        ExtractedSkills = o.ExtractedSkills,
        ExtractedKeywords = o.ExtractedKeywords,
        CreatedAt = o.CreatedAt
    };
}