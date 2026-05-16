namespace AiCv.Api.Modules.Profiles.Entities;
public class Certification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty; // ex: AWS, Cisco, Coursera
    public string Year { get; set; } = string.Empty; 
}