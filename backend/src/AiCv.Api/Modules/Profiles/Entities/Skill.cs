namespace AiCv.Api.Modules.Profiles.Entities;
public class Skill
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty; // ex: Débutant, Intermédiaire, Expert
    public string Category { get; set; } = string.Empty; // ex: Hard Skill, Soft Skill
}