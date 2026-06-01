using System.ComponentModel.DataAnnotations;

namespace AiCv.OpportunityService.Modules.Opportunities.DTOs;

public class UpdateOpportunityDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string CompanyName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
