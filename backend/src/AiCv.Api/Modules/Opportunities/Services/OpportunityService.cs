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

    public async Task<OpportunityResponseDto?> CreateAsync(string keycloakId, CreateOpportunityDto dto)
    {
        var userId = await ResolveUserIdAsync(keycloakId);
        if (userId is null)
        {
            return null;
        }

        var jobOffer = new JobOffer
        {
            UserId = userId.Value,
            Title = dto.Title.Trim(),
            CompanyName = dto.CompanyName.Trim(),
            Description = dto.Description.Trim(),
            AnalysisStatus = "pending",
            UpdatedAt = DateTime.UtcNow,
        };

        await _repository.AddAsync(jobOffer);
        await _repository.SaveChangesAsync();

        return MapToResponseDto(jobOffer);
    }

    public async Task<List<OpportunityListItemDto>?> GetAllByUserAsync(string keycloakId)
    {
        var userId = await ResolveUserIdAsync(keycloakId);
        if (userId is null)
        {
            return null;
        }

        var jobOffers = await _repository.GetAllByUserIdAsync(userId.Value);

        return jobOffers.Select(jobOffer => new OpportunityListItemDto
        {
            Id = jobOffer.Id,
            Title = jobOffer.Title,
            CompanyName = jobOffer.CompanyName,
            AnalysisStatus = jobOffer.AnalysisStatus,
            CreatedAt = jobOffer.CreatedAt,
            UpdatedAt = jobOffer.UpdatedAt,
        }).ToList();
    }

    public async Task<OpportunityResponseDto?> GetByIdAsync(Guid id, string keycloakId)
    {
        var userId = await ResolveUserIdAsync(keycloakId);
        if (userId is null)
        {
            return null;
        }

        var jobOffer = await _repository.GetByIdAndUserIdAsync(id, userId.Value);
        return jobOffer is null ? null : MapToResponseDto(jobOffer);
    }

    public async Task<OpportunityResponseDto?> UpdateAsync(Guid id, string keycloakId, UpdateOpportunityDto dto)
    {
        var userId = await ResolveUserIdAsync(keycloakId);
        if (userId is null)
        {
            return null;
        }

        var jobOffer = await _repository.GetByIdAndUserIdAsync(id, userId.Value);
        if (jobOffer is null)
        {
            return null;
        }

        jobOffer.Title = dto.Title.Trim();
        jobOffer.CompanyName = dto.CompanyName.Trim();
        jobOffer.Description = dto.Description.Trim();
        jobOffer.AnalysisStatus = "pending";
        jobOffer.UpdatedAt = DateTime.UtcNow;

        if (jobOffer.Analysis is not null)
        {
            _repository.RemoveAnalysis(jobOffer.Analysis);
            jobOffer.Analysis = null;
        }

        await _repository.SaveChangesAsync();
        return MapToResponseDto(jobOffer);
    }

    public async Task<bool> DeleteAsync(Guid id, string keycloakId)
    {
        var userId = await ResolveUserIdAsync(keycloakId);
        if (userId is null)
        {
            return false;
        }

        var jobOffer = await _repository.GetByIdAndUserIdAsync(id, userId.Value);
        if (jobOffer is null)
        {
            return false;
        }

        _repository.Remove(jobOffer);
        await _repository.SaveChangesAsync();
        return true;
    }

    private Task<Guid?> ResolveUserIdAsync(string keycloakId)
    {
        return _repository.GetUserIdByKeycloakIdAsync(keycloakId);
    }

    private static OpportunityResponseDto MapToResponseDto(JobOffer jobOffer) => new()
    {
        Id = jobOffer.Id,
        UserId = jobOffer.UserId,
        Title = jobOffer.Title,
        CompanyName = jobOffer.CompanyName,
        Description = jobOffer.Description,
        AnalysisStatus = jobOffer.AnalysisStatus,
        CreatedAt = jobOffer.CreatedAt,
        UpdatedAt = jobOffer.UpdatedAt,
        Analysis = jobOffer.Analysis is null
            ? null
            : new OpportunityAnalysisResponseDto
            {
                Id = jobOffer.Analysis.Id,
                JobOfferId = jobOffer.Analysis.JobOfferId,
                ExtractedSkills = jobOffer.Analysis.ExtractedSkills,
                ExtractedKeywords = jobOffer.Analysis.ExtractedKeywords,
                ExtractedResponsibilities = jobOffer.Analysis.ExtractedResponsibilities,
                DetectedExperienceLevel = jobOffer.Analysis.DetectedExperienceLevel,
                DetectedLocation = jobOffer.Analysis.DetectedLocation,
                DetectedContractType = jobOffer.Analysis.DetectedContractType,
                DetectedTechnologies = jobOffer.Analysis.DetectedTechnologies,
                AnalysisSummary = jobOffer.Analysis.AnalysisSummary,
                RawAnalysisJson = jobOffer.Analysis.RawAnalysisJson,
                CreatedAt = jobOffer.Analysis.CreatedAt,
            },
    };
}
