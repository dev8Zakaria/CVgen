using AiCv.Api.Modules.Profiles.Entities;

namespace AiCv.Api.Modules.Cvs.Entities;

public class Cv
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    public Guid OpportunityId { get; set; }
    
    // Le lien vers le fichier stocké dans MinIO
    public string? AssetUrl { get; set; } 
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}