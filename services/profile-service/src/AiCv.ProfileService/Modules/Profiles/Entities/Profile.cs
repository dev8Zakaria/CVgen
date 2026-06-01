using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AiCv.ProfileService.Modules.Profiles.Entities;

public class Profile
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    [JsonIgnore]
    public User? User { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Experience> Experiences { get; set; } = new List<Experience>();
    public ICollection<Education> Educations { get; set; } = new List<Education>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    public ICollection<Language> Languages { get; set; } = new List<Language>();
    public ICollection<Certification> Certifications { get; set; } = new List<Certification>();
}
