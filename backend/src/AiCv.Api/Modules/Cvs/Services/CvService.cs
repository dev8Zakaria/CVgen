using AiCv.Api.Modules.Cvs.DTOs;
using AiCv.Api.Modules.Cvs.Entities;
using AiCv.Api.Modules.Cvs.Repositories;
using AiCv.Api.Modules.Opportunities.Repositories;
using AiCv.Api.Modules.Profiles.Repositories;
using AiCv.Api.Shared.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace AiCv.Api.Modules.Cvs.Services;

public class CvService
{
    private readonly CvRepository _cvRepository;
    private readonly ProfileRepository _profileRepository;
    private readonly OpportunityRepository _opportunityRepository;
    private readonly IServiceProvider _serviceProvider;

    public CvService(
        CvRepository cvRepository,
        ProfileRepository profileRepository,
        OpportunityRepository opportunityRepository,
        IServiceProvider serviceProvider)
    {
        _cvRepository = cvRepository;
        _profileRepository = profileRepository;
        _opportunityRepository = opportunityRepository;
        _serviceProvider = serviceProvider;
    }

    public async Task<GeneratedCvResponseDto?> GenerateInitialCvAsync(string keycloakId, GenerateCvRequestDto request)
    {
        var user = await _profileRepository.GetUserWithProfileByKeycloakIdAsync(keycloakId);
        if (user == null) return null;

        var opportunity = await _opportunityRepository.GetByIdAndUserIdAsync(request.OpportunityId, user.Id);
        if (opportunity?.Analysis == null || opportunity.AnalysisStatus != "completed") return null;

        string? assetUrl = null;

        if (request.AssetFile != null && request.AssetFile.Length > 0)
        {
            var storageService = _serviceProvider.GetRequiredService<MinioStorageService>();
            using var stream = request.AssetFile.OpenReadStream();
            assetUrl = await storageService.UploadFileAsync(stream, request.AssetFile.FileName, request.AssetFile.ContentType);
        }

        var cv = new Cv
        {
            UserId = user.Id,
            OpportunityId = request.OpportunityId,
            AssetUrl = assetUrl
        };

        await _cvRepository.AddCvAsync(cv);
        await _cvRepository.SaveChangesAsync();

        return new GeneratedCvResponseDto
        {
            CvId = cv.Id,
            AssetUrl = cv.AssetUrl,
            GeneratedAt = cv.CreatedAt,
            ProfessionalSummary = BuildProfessionalSummary(user.Profile?.Summary, opportunity.Analysis.AnalysisSummary),
            HighlightedSkills = opportunity.Analysis.ExtractedSkills,
            MatchingKeywords = opportunity.Analysis.ExtractedKeywords,
            TailoredExperienceHints = opportunity.Analysis.CvFocusPoints,

            Profile = new GeneratedCvProfileDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Profile?.Phone ?? string.Empty,
                Location = user.Profile?.Location ?? string.Empty,
                Headline = user.Profile?.Title ?? string.Empty
            },

            Target = new GeneratedCvTargetDto
            {
                OpportunityId = request.OpportunityId,
                JobTitle = opportunity.Title,
                CompanyName = opportunity.CompanyName
            }
        };
    }

    private static string BuildProfessionalSummary(string? profileSummary, string analysisSummary)
    {
        if (string.IsNullOrWhiteSpace(profileSummary))
        {
            return analysisSummary;
        }

        if (string.IsNullOrWhiteSpace(analysisSummary))
        {
            return profileSummary;
        }

        return $"{profileSummary}\n\n{analysisSummary}";
    }
}
