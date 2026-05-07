using AiCv.Api.Modules.Cvs.DTOs;
using AiCv.Api.Modules.Cvs.Repositories;
using AiCv.Api.Modules.Opportunities.Entities;
using AiCv.Api.Modules.Profiles.Entities;

namespace AiCv.Api.Modules.Cvs.Services;

public sealed class CvService
{
    private readonly CvRepository _repository;

    public CvService(CvRepository repository)
    {
        _repository = repository;
    }

    public async Task<GeneratedCvResponseDto?> GenerateAsync(Guid opportunityId, string keycloakId)
    {
        var user = await _repository.GetUserWithProfileByKeycloakIdAsync(keycloakId);
        if (user?.Profile is null)
        {
            return null;
        }

        var opportunity = await _repository.GetAnalyzedOpportunityByIdAndUserIdAsync(opportunityId, user.Id);
        if (opportunity?.Analysis is null || !string.Equals(opportunity.AnalysisStatus, "completed", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return BuildResponse(user, user.Profile, opportunity, opportunity.Analysis);
    }

    private static GeneratedCvResponseDto BuildResponse(
        User user,
        Profile profile,
        JobOffer opportunity,
        JobOfferAnalysis analysis)
    {
        var highlightedSkills = analysis.ExtractedSkills
            .Concat(analysis.DetectedTechnologies)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList();

        var keywords = analysis.ExtractedKeywords
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList();

        var summaryParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(profile.Summary))
        {
            summaryParts.Add(profile.Summary.Trim());
        }

        summaryParts.Add(
            $"Targeting the {opportunity.Title} role at {opportunity.CompanyName} with focus on {BuildFocusPhrase(highlightedSkills, keywords)}.");

        if (analysis.MatchScorePhrase() is { Length: > 0 } matchScorePhrase)
        {
            summaryParts.Add(matchScorePhrase);
        }

        var experienceHints = new List<string>
        {
            $"Adapt your project bullets to emphasize {BuildFocusPhrase(highlightedSkills, keywords)}.",
            $"Mirror the vocabulary from the job offer for {opportunity.Title} at {opportunity.CompanyName}.",
        };

        if (keywords.Count > 0)
        {
            experienceHints.Add($"Ensure the CV references these keywords explicitly: {string.Join(", ", keywords)}.");
        }

        return new GeneratedCvResponseDto
        {
            Profile = new GeneratedCvProfileDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = profile.Phone,
                Location = profile.Location,
                Headline = string.IsNullOrWhiteSpace(profile.Title) ? opportunity.Title : profile.Title,
            },
            Target = new GeneratedCvTargetDto
            {
                OpportunityId = opportunity.Id,
                JobTitle = opportunity.Title,
                CompanyName = opportunity.CompanyName,
            },
            ProfessionalSummary = string.Join(" ", summaryParts),
            HighlightedSkills = highlightedSkills,
            MatchingKeywords = keywords,
            TailoredExperienceHints = experienceHints,
            GeneratedAt = DateTime.UtcNow,
        };
    }

    private static string BuildFocusPhrase(List<string> highlightedSkills, List<string> keywords)
    {
        var focusItems = highlightedSkills.Concat(keywords).Distinct(StringComparer.OrdinalIgnoreCase).Take(3).ToList();
        return focusItems.Count > 0 ? string.Join(", ", focusItems) : "the role requirements";
    }
}

internal static class CvAnalysisExtensions
{
    public static string MatchScorePhrase(this JobOfferAnalysis analysis)
    {
        const string prefix = "Estimated match score:";
        if (string.IsNullOrWhiteSpace(analysis.AnalysisSummary) || !analysis.AnalysisSummary.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return analysis.AnalysisSummary.TrimEnd('.') + ".";
    }
}
