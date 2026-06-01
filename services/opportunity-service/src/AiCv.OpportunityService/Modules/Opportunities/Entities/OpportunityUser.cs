using System.ComponentModel.DataAnnotations;

namespace AiCv.OpportunityService.Modules.Opportunities.Entities;

public class OpportunityUser
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string KeycloakId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<JobOffer> JobOffers { get; set; } = new List<JobOffer>();
}
