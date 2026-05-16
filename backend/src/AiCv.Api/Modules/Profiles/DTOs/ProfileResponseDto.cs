namespace AiCv.Api.Modules.Profiles.DTOs;

public class ProfileResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }

    public List<ExperienceResponseDto> Experiences { get; set; } = new();
    public List<EducationResponseDto> Educations { get; set; } = new();
    public List<ProjectResponseDto> Projects { get; set; } = new();
    public List<SkillResponseDto> Skills { get; set; } = new();
    public List<LanguageResponseDto> Languages { get; set; } = new();
    public List<CertificationResponseDto> Certifications { get; set; } = new();
}

public class ExperienceResponseDto
{
    public Guid Id { get; set; }
    public string Company { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class EducationResponseDto
{
    public Guid Id { get; set; }
    public string School { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class ProjectResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Technologies { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class SkillResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class LanguageResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
}

public class CertificationResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
}