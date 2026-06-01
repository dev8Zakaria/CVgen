namespace AiCv.ProfileService.Modules.Profiles.Entities;

public class Certification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
}
