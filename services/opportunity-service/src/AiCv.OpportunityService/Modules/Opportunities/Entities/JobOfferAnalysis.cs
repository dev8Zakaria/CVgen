using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AiCv.OpportunityService.Modules.Opportunities.Entities;

public class JobOfferAnalysis
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(JobOffer))]
    public Guid JobOfferId { get; set; }

    [JsonIgnore]
    public JobOffer? JobOffer { get; set; }

    public List<string> ExtractedSkills { get; set; } = [];
    public List<string> ExtractedKeywords { get; set; } = [];
    public List<string> ExtractedResponsibilities { get; set; } = [];
    public string DetectedExperienceLevel { get; set; } = string.Empty;
    public string DetectedLocation { get; set; } = string.Empty;
    public string DetectedContractType { get; set; } = string.Empty;
    public List<string> DetectedTechnologies { get; set; } = [];
    public List<string> MustHaveRequirements { get; set; } = [];
    public List<string> NiceToHaveRequirements { get; set; } = [];
    public List<string> CvFocusPoints { get; set; } = [];
    public List<string> CandidateRisks { get; set; } = [];
    public string AnalysisSummary { get; set; } = string.Empty;
    public int MatchScoreEstimation { get; set; }
    public double ConfidenceScore { get; set; }
    public string ReasoningSummary { get; set; } = string.Empty;
    public string RawAnalysisJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
