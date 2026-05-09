using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using AiCv.Api.Modules.Profiles.Entities;

namespace AiCv.Api.Modules.Opportunities.Entities;

public class JobOffer
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey("User")]
    public Guid UserId { get; set; }

    [JsonIgnore]
    public User? User { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string CompanyName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    public string AnalysisStatus { get; set; } = "pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public JobOfferAnalysis? Analysis { get; set; }
}
