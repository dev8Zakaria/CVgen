using AiCv.ProfileService.Modules.Profiles.DTOs;
using AiCv.ProfileService.Modules.Profiles.Entities;
using AiCv.ProfileService.Modules.Profiles.Repositories;

namespace AiCv.ProfileService.Modules.Profiles.Services;

public class ProfileService
{
    private readonly ProfileRepository _repository;

    public ProfileService(ProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProfileResponseDto> GetOrCreateProfileAsync(string keycloakId, string email, string fullName)
    {
        var user = await _repository.GetUserWithProfileByKeycloakIdAsync(keycloakId);

        if (user?.Profile == null)
        {
            user = new User
            {
                KeycloakId = keycloakId,
                Email = email,
                FullName = fullName,
                Role = "User"
            };

            user.Profile = new Profile
                {
                    UserId = user.Id,
                    Title = "Nouveau Profil",
                    Summary = "Genere automatiquement suite a l'inscription."
                };

            await _repository.AddUserAsync(user);
            await _repository.SaveChangesAsync();
        }

        return MapToResponseDto(user);
    }

    public async Task<ProfileResponseDto?> UpdateProfileAsync(string keycloakId, ProfileUpdateDto request)
    {
        var user = await _repository.GetUserWithProfileByKeycloakIdAsync(keycloakId);
        if (user?.Profile == null)
        {
            return null;
        }

        user.Profile.Title = request.Title;
        user.Profile.Summary = request.Summary;
        user.Profile.Phone = request.Phone;
        user.Profile.Location = request.Location;
        user.Profile.UpdatedAt = DateTime.UtcNow;

        SyncExperiences(user.Profile, request.Experiences);
        SyncEducations(user.Profile, request.Educations);
        SyncProjects(user.Profile, request.Projects);
        SyncSkills(user.Profile, request.Skills);
        SyncLanguages(user.Profile, request.Languages);
        SyncCertifications(user.Profile, request.Certifications);

        await _repository.SaveChangesAsync();

        return MapToResponseDto(user);
    }

    public async Task<bool> DeleteProfileAsync(string keycloakId)
    {
        var user = await _repository.GetUserWithProfileByKeycloakIdAsync(keycloakId);
        if (user == null)
        {
            return false;
        }

        _repository.RemoveUser(user);
        await _repository.SaveChangesAsync();

        return true;
    }

    private static void SyncExperiences(Profile profile, List<ExperienceUpdateDto> dtos)
    {
        var incomingIds = dtos.Where(dto => dto.Id.HasValue).Select(dto => dto.Id!.Value).ToHashSet();
        RemoveMissing(profile.Experiences, incomingIds);

        foreach (var dto in dtos)
        {
            var existing = dto.Id.HasValue ? profile.Experiences.FirstOrDefault(item => item.Id == dto.Id.Value) : null;

            if (existing == null)
            {
                profile.Experiences.Add(new Experience
                {
                    Id = Guid.Empty,
                    ProfileId = profile.Id,
                    Company = dto.Company,
                    Position = dto.Position,
                    StartDate = ToUtc(dto.StartDate),
                    EndDate = ToUtc(dto.EndDate),
                    Description = dto.Description
                });
                continue;
            }

            existing.Company = dto.Company;
            existing.Position = dto.Position;
            existing.StartDate = ToUtc(dto.StartDate);
            existing.EndDate = ToUtc(dto.EndDate);
            existing.Description = dto.Description;
        }
    }

    private static void SyncEducations(Profile profile, List<EducationUpdateDto> dtos)
    {
        var incomingIds = dtos.Where(dto => dto.Id.HasValue).Select(dto => dto.Id!.Value).ToHashSet();
        RemoveMissing(profile.Educations, incomingIds);

        foreach (var dto in dtos)
        {
            var existing = dto.Id.HasValue ? profile.Educations.FirstOrDefault(item => item.Id == dto.Id.Value) : null;

            if (existing == null)
            {
                profile.Educations.Add(new Education
                {
                    Id = Guid.Empty,
                    ProfileId = profile.Id,
                    School = dto.School,
                    Degree = dto.Degree,
                    Field = dto.Field,
                    StartDate = ToUtc(dto.StartDate),
                    EndDate = ToUtc(dto.EndDate)
                });
                continue;
            }

            existing.School = dto.School;
            existing.Degree = dto.Degree;
            existing.Field = dto.Field;
            existing.StartDate = ToUtc(dto.StartDate);
            existing.EndDate = ToUtc(dto.EndDate);
        }
    }

    private static void SyncProjects(Profile profile, List<ProjectUpdateDto> dtos)
    {
        var incomingIds = dtos.Where(dto => dto.Id.HasValue).Select(dto => dto.Id!.Value).ToHashSet();
        RemoveMissing(profile.Projects, incomingIds);

        foreach (var dto in dtos)
        {
            var existing = dto.Id.HasValue ? profile.Projects.FirstOrDefault(item => item.Id == dto.Id.Value) : null;

            if (existing == null)
            {
                profile.Projects.Add(new Project
                {
                    Id = Guid.Empty,
                    ProfileId = profile.Id,
                    Name = dto.Name,
                    Description = dto.Description,
                    Technologies = dto.Technologies,
                    Url = dto.Url
                });
                continue;
            }

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.Technologies = dto.Technologies;
            existing.Url = dto.Url;
        }
    }

    private static void SyncSkills(Profile profile, List<SkillUpdateDto> dtos)
    {
        var incomingIds = dtos.Where(dto => dto.Id.HasValue).Select(dto => dto.Id!.Value).ToHashSet();
        RemoveMissing(profile.Skills, incomingIds);

        foreach (var dto in dtos)
        {
            var existing = dto.Id.HasValue ? profile.Skills.FirstOrDefault(item => item.Id == dto.Id.Value) : null;

            if (existing == null)
            {
                profile.Skills.Add(new Skill
                {
                    Id = Guid.Empty,
                    ProfileId = profile.Id,
                    Name = dto.Name,
                    Level = dto.Level,
                    Category = dto.Category
                });
                continue;
            }

            existing.Name = dto.Name;
            existing.Level = dto.Level;
            existing.Category = dto.Category;
        }
    }

    private static void SyncLanguages(Profile profile, List<LanguageUpdateDto> dtos)
    {
        var incomingIds = dtos.Where(dto => dto.Id.HasValue).Select(dto => dto.Id!.Value).ToHashSet();
        RemoveMissing(profile.Languages, incomingIds);

        foreach (var dto in dtos)
        {
            var existing = dto.Id.HasValue ? profile.Languages.FirstOrDefault(item => item.Id == dto.Id.Value) : null;

            if (existing == null)
            {
                profile.Languages.Add(new Language
                {
                    Id = Guid.Empty,
                    ProfileId = profile.Id,
                    Name = dto.Name,
                    Level = dto.Level
                });
                continue;
            }

            existing.Name = dto.Name;
            existing.Level = dto.Level;
        }
    }

    private static void SyncCertifications(Profile profile, List<CertificationUpdateDto> dtos)
    {
        var incomingIds = dtos.Where(dto => dto.Id.HasValue).Select(dto => dto.Id!.Value).ToHashSet();
        RemoveMissing(profile.Certifications, incomingIds);

        foreach (var dto in dtos)
        {
            var existing = dto.Id.HasValue ? profile.Certifications.FirstOrDefault(item => item.Id == dto.Id.Value) : null;

            if (existing == null)
            {
                profile.Certifications.Add(new Certification
                {
                    Id = Guid.Empty,
                    ProfileId = profile.Id,
                    Title = dto.Title,
                    Issuer = dto.Issuer,
                    Year = dto.Year
                });
                continue;
            }

            existing.Title = dto.Title;
            existing.Issuer = dto.Issuer;
            existing.Year = dto.Year;
        }
    }

    private static void RemoveMissing<T>(ICollection<T> currentItems, HashSet<Guid> incomingIds) where T : class
    {
        var itemsToRemove = currentItems
            .Where(item => !incomingIds.Contains((Guid)item.GetType().GetProperty("Id")!.GetValue(item)!))
            .ToList();

        foreach (var item in itemsToRemove)
        {
            currentItems.Remove(item);
        }
    }

    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }

    private static DateTime? ToUtc(DateTime? value)
    {
        return value.HasValue ? ToUtc(value.Value) : null;
    }

    private static ProfileResponseDto MapToResponseDto(User user)
    {
        var profile = user.Profile!;

        return new ProfileResponseDto
        {
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            Id = profile.Id,
            Title = profile.Title,
            Summary = profile.Summary,
            Phone = profile.Phone,
            Location = profile.Location,
            UpdatedAt = profile.UpdatedAt,
            Experiences = profile.Experiences.Select(item => new ExperienceResponseDto { Id = item.Id, Company = item.Company, Position = item.Position, StartDate = item.StartDate, EndDate = item.EndDate, Description = item.Description }).ToList(),
            Educations = profile.Educations.Select(item => new EducationResponseDto { Id = item.Id, School = item.School, Degree = item.Degree, Field = item.Field, StartDate = item.StartDate, EndDate = item.EndDate }).ToList(),
            Projects = profile.Projects.Select(item => new ProjectResponseDto { Id = item.Id, Name = item.Name, Description = item.Description, Technologies = item.Technologies, Url = item.Url }).ToList(),
            Skills = profile.Skills.Select(item => new SkillResponseDto { Id = item.Id, Name = item.Name, Level = item.Level, Category = item.Category }).ToList(),
            Languages = profile.Languages.Select(item => new LanguageResponseDto { Id = item.Id, Name = item.Name, Level = item.Level }).ToList(),
            Certifications = profile.Certifications.Select(item => new CertificationResponseDto { Id = item.Id, Title = item.Title, Issuer = item.Issuer, Year = item.Year }).ToList()
        };
    }
}
