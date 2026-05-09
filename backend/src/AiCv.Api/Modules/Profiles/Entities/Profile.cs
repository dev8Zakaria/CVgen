using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // <--- 1. AJOUTER CECI

namespace AiCv.Api.Modules.Profiles.Entities;

public class Profile
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey("User")]
    public Guid UserId { get; set; }

    [JsonIgnore] // <--- 2. AJOUTER CECI POUR CASSER LA BOUCLE
    public User? User { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}