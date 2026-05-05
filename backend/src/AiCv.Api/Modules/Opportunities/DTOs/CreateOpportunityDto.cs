using System.ComponentModel.DataAnnotations;

namespace AiCv.Api.Modules.Opportunities.DTOs;

public class CreateOpportunityDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Company { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<string> ExtractedSkills { get; set; } = new();

    public List<string> ExtractedKeywords { get; set; } = new();
}