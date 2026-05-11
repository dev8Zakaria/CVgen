using System.Text.Json;
using AiCv.Api.Modules.Ai;
using AiCv.Api.Modules.Ai.DTOs;
using AiCv.Api.Modules.Opportunities.DTOs;
using AiCv.Api.Modules.Opportunities.Entities;
using AiCv.Api.Modules.Opportunities.Repositories;

namespace AiCv.Api.Modules.Opportunities.Services;

public class OpportunityService
{
    private readonly OpportunityRepository _repository;
    private readonly IAiService _aiService;

    public OpportunityService(OpportunityRepository repository, IAiService aiService)
    {
        _repository = repository;
        _aiService = aiService;
    }

    public async Task<OpportunityDetailsResponseDto?> CreateAsync(string keycloakId, CreateOpportunityDto dto)
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

    public async Task<List<OpportunityListResponseDto>?> GetAllByUserAsync(string keycloakId)
    {
        var userId = await ResolveUserIdAsync(keycloakId);
        if (userId is null)
        {
            return null;
        }

        var jobOffers = await _repository.GetAllByUserIdAsync(userId.Value);

        return jobOffers.Select(jobOffer => new OpportunityListResponseDto
        {
            Id = jobOffer.Id,
            Title = jobOffer.Title,
            CompanyName = jobOffer.CompanyName,
            AnalysisStatus = jobOffer.AnalysisStatus,
            CreatedAt = jobOffer.CreatedAt,
            UpdatedAt = jobOffer.UpdatedAt,
        }).ToList();
    }

    public async Task<OpportunityDetailsResponseDto?> GetByIdAsync(Guid id, string keycloakId)
    {
        var userId = await ResolveUserIdAsync(keycloakId);
        if (userId is null)
        {
            return null;
        }

        var jobOffer = await _repository.GetByIdAndUserIdAsync(id, userId.Value);
        return jobOffer is null ? null : MapToResponseDto(jobOffer);
    }

    public async Task<OpportunityDetailsResponseDto?> UpdateAsync(Guid id, string keycloakId, UpdateOpportunityDto dto)
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

    public async Task<OpportunityDetailsResponseDto?> AnalyzeAsync(Guid id, string keycloakId, CancellationToken cancellationToken = default)
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

        jobOffer.AnalysisStatus = "processing";
        jobOffer.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync();

        try
        {
            var aiResponse = await _aiService.AnalyzeJobAsync(jobOffer.Description, cancellationToken);
            await UpsertAnalysisAsync(jobOffer, aiResponse);

            jobOffer.AnalysisStatus = "completed";
            jobOffer.UpdatedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync();

            return MapToResponseDto(jobOffer);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException or AiAnalysisFailedException)
        {
            jobOffer.AnalysisStatus = "failed";
            jobOffer.UpdatedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync();

            throw new AiAnalysisFailedException("AI analysis failed for this job offer.", exception);
        }
    }

    private Task<Guid?> ResolveUserIdAsync(string keycloakId)
    {
        return _repository.GetUserIdByKeycloakIdAsync(keycloakId);
    }

    private async Task UpsertAnalysisAsync(JobOffer jobOffer, AiJobAnalysisResponseDto aiResponse)
    {
        var analysis = jobOffer.Analysis;
        if (analysis is null)
        {
            analysis = new JobOfferAnalysis
            {
                JobOfferId = jobOffer.Id,
            };

            await _repository.AddAnalysisAsync(analysis);
            jobOffer.Analysis = analysis;
        }

        analysis.ExtractedKeywords = aiResponse.ExtractedKeywords;
        analysis.ExtractedSkills = aiResponse.SuggestedSkills;
        analysis.ExtractedResponsibilities = aiResponse.ExtractedResponsibilities;
        analysis.DetectedExperienceLevel = aiResponse.DetectedExperienceLevel;
        analysis.DetectedLocation = aiResponse.DetectedLocation;
        analysis.DetectedContractType = aiResponse.DetectedContractType;
        analysis.DetectedTechnologies = aiResponse.DetectedTechnologies;
        analysis.MustHaveRequirements = aiResponse.MustHaveRequirements;
        analysis.NiceToHaveRequirements = aiResponse.NiceToHaveRequirements;
        analysis.CvFocusPoints = aiResponse.CvFocusPoints;
        analysis.CandidateRisks = aiResponse.CandidateRisks;
        analysis.AnalysisSummary = string.IsNullOrWhiteSpace(aiResponse.AnalysisSummary)
            ? $"Estimated match score: {aiResponse.MatchScoreEstimation}%"
            : aiResponse.AnalysisSummary.Trim();
        analysis.MatchScoreEstimation = aiResponse.MatchScoreEstimation;
        analysis.ConfidenceScore = aiResponse.ConfidenceScore;
        analysis.ReasoningSummary = aiResponse.ReasoningSummary;
        analysis.RawAnalysisJson = JsonSerializer.Serialize(aiResponse);
        analysis.CreatedAt = DateTime.UtcNow;
    }

    private static OpportunityDetailsResponseDto MapToResponseDto(JobOffer jobOffer) => new()
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
            : new OpportunityAnalysisDto
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
                MustHaveRequirements = jobOffer.Analysis.MustHaveRequirements,
                NiceToHaveRequirements = jobOffer.Analysis.NiceToHaveRequirements,
                CvFocusPoints = jobOffer.Analysis.CvFocusPoints,
                CandidateRisks = jobOffer.Analysis.CandidateRisks,
                AnalysisSummary = jobOffer.Analysis.AnalysisSummary,
                MatchScoreEstimation = jobOffer.Analysis.MatchScoreEstimation,
                ConfidenceScore = jobOffer.Analysis.ConfidenceScore,
                ReasoningSummary = jobOffer.Analysis.ReasoningSummary,
                RawAnalysisJson = jobOffer.Analysis.RawAnalysisJson,
                CreatedAt = jobOffer.Analysis.CreatedAt,
            },
    };
}
