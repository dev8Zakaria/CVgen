namespace AiCv.Api.Modules.Profiles.DTOs;

public class ProfileUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public List<ExperienceUpdateDto> Experiences { get; set; } = new();
    public List<EducationUpdateDto> Educations { get; set; } = new();
    public List<ProjectUpdateDto> Projects { get; set; } = new();
    public List<SkillUpdateDto> Skills { get; set; } = new();
    public List<LanguageUpdateDto> Languages { get; set; } = new();
    public List<CertificationUpdateDto> Certifications { get; set; } = new();
}

public class ExperienceUpdateDto
{
    public Guid? Id { get; set; } // Optionnel pour savoir si c'est un ajout ou une modif
    public string Company { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class EducationUpdateDto
{
    public Guid? Id { get; set; }
    public string School { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class ProjectUpdateDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Technologies { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class SkillUpdateDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class LanguageUpdateDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
}

public class CertificationUpdateDto
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
}