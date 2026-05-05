using AiCv.Api.Modules.Opportunities.DTOs;
using AiCv.Api.Modules.Opportunities.Entities;
using AiCv.Api.Modules.Opportunities.Repositories;

namespace AiCv.Api.Modules.Opportunities.Services;

public class OpportunityService
{
    private readonly OpportunityRepository _repository;

    public OpportunityService(OpportunityRepository repository)
    {
        _repository = repository;
    }

    // 1. CREATE
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

        await _repository.AddAsync(opportunity);
        await _repository.SaveChangesAsync();

        return MapToResponseDto(opportunity);
    }

    // 2. GET ALL — retourne uniquement title + company
    public async Task<List<OpportunityListItemDto>> GetAllByUserAsync(Guid userId)
    {
        var opportunities = await _repository.GetAllByUserIdAsync(userId);

        return opportunities.Select(o => new OpportunityListItemDto
        {
            Id = o.Id,
            Title = o.Title,
            Company = o.Company,
            CreatedAt = o.CreatedAt
        }).ToList();
    }

    // 3. GET BY ID — retourne le détail complet
    public async Task<OpportunityResponseDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var opportunity = await _repository.GetByIdAndUserIdAsync(id, userId);
        return opportunity is null ? null : MapToResponseDto(opportunity);
    }

    // Mapping privé vers le DTO complet
    private static OpportunityResponseDto MapToResponseDto(Opportunity o) => new()
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