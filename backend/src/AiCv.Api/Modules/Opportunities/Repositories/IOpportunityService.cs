using AiCv.Api.Modules.Opportunities.DTOs;

namespace AiCv.Api.Modules.Opportunities.Repositories;

public interface IOpportunityService
{
    Task<OpportunityResponseDto> CreateAsync(Guid userId, CreateOpportunityDto dto);
    Task<List<OpportunityResponseDto>> GetAllByUserAsync(Guid userId);
    Task<OpportunityResponseDto?> GetByIdAsync(Guid id, Guid userId);
}