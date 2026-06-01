using System.ComponentModel.DataAnnotations;

namespace AiCv.ProfileService.Modules.Profiles.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string KeycloakId { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Profile? Profile { get; set; }
}
