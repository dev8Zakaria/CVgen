namespace AiCv.Api.Modules.Profiles.Entities;
public class Language
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty; // ex: B2, Courant, Maternel
}