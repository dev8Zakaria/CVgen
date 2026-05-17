using AiCv.Api.Modules.Profiles.DTOs;
using AiCv.Api.Modules.Profiles.Entities;
using AiCv.Api.Modules.Profiles.Repositories;
using Minio.DataModel.Replication;

namespace AiCv.Api.Modules.Profiles.Services;

public class ProfileService
{
    private readonly ProfileRepository _repository;

    public ProfileService(ProfileRepository repository)
    {
        _repository = repository;
    }

    // 1. GET ou CREATE
    public async Task<ProfileResponseDto> GetOrCreateProfileAsync(string keycloakId, string email, string fullName)
    {
        var user = await _repository.GetUserWithProfileByKeycloakIdAsync(keycloakId);

        if (user == null || user.Profile == null)
        {
            user = new User
            {
                KeycloakId = keycloakId,
                Email = email,
                FullName = fullName,
                Role = "User",
                Profile = new Profile
                {
                    Title = "Nouveau Profil",
                    Summary = "Généré automatiquement suite à l'inscription."
                }
            };
            await _repository.AddUserAsync(user);
            await _repository.SaveChangesAsync();
        }

        return MapToResponseDto(user);
    }

    // 2. UPDATE
    public async Task<ProfileResponseDto?> UpdateProfileAsync(string keycloakId, ProfileUpdateDto request)
    {
        var user = await _repository.GetUserWithProfileByKeycloakIdAsync(keycloakId);
        if (user?.Profile == null) return null;

        // 1. Mise à jour des champs de base
        user.Profile.Title = request.Title;
        user.Profile.Summary = request.Summary;
        user.Profile.Phone = request.Phone;
        user.Profile.Location = request.Location;
        user.Profile.UpdatedAt = DateTime.UtcNow;

        // 2. Synchronisation des Expériences
        await SyncExperiences(user.Profile, request.Experiences);
        
        // 3. Synchronisation des Formations
        await SyncEducations(user.Profile, request.Educations);

        // 4. Synchronisation des Projets
        await SyncProjects(user.Profile, request.Projects);

        // 5. Synchronisation des Compétences
        await SyncSkills(user.Profile, request.Skills);
        await SyncLanguages(user.Profile, request.Languages);
        await SyncCertifications(user.Profile, request.Certifications);

        // 6. Sauvegarde globale (Une seule transaction !)
        await _repository.SaveChangesAsync();

        // Remarque: Mappez manuellement ou avec AutoMapper vers votre ProfileResponseDto final
        return MapToResponseDto(user);
    }

    // --- Fonctions privées pour garder le code propre ---
// ========================================================================
    // --- FONCTIONS DE SYNCHRONISATION SÉCURISÉES (ANTI-CRASH EF CORE) ---
    // ========================================================================

    private async Task SyncExperiences(Profile profile, List<ExperienceUpdateDto> dtos)
    {
        // 1. SUPPRESSION
    var incomingIds = dtos.Where(d => d.Id.HasValue).Select(d => d.Id!.Value).ToList();
    var itemsToRemove = profile.Experiences.Where(e => !incomingIds.Contains(e.Id)).ToList();
    foreach (var item in itemsToRemove)
    {
        profile.Experiences.Remove(item);
    }
       foreach( ExperienceUpdateDto d in dtos)
        {
            if (d.Id == null)
            {
                Experience? e=new Experience();
                e.Id=Guid.Empty;
                e.ProfileId=profile.Id;
                e.Company=d.Company;
                e.Description=d.Description;
                e.EndDate=d.EndDate?.ToUniversalTime();
                e.Position=d.Position;
                e.StartDate=d.StartDate.ToUniversalTime();
                profile.Experiences.Add(e);
            }
            else
            {
                var existingExperience = profile.Experiences.FirstOrDefault(e => e.Id == d.Id);

            // Si elle n'existe pas, on lève une erreur (Exception)
                    if (existingExperience == null)
                    {
                        throw new Exception($"Erreur : L'expérience avec l'ID {d.Id} n'existe pas dans la base de données.");
                    }

                    // Si elle existe, on modifie les champs
                    existingExperience.Company = d.Company;
                    existingExperience.Description = d.Description;
                    existingExperience.EndDate = d.EndDate?.ToUniversalTime();
                    existingExperience.Position = d.Position;
                    existingExperience.StartDate = d.StartDate.ToUniversalTime();

                    // Note : Avec Entity Framework, modifier les propriétés d'un objet tracké 
                    // suffit. Pas besoin d'appeler _repository.Update().
                }
            }
            
    }

    private async Task SyncEducations(Profile profile, List<EducationUpdateDto> dtos)
    {
        // 1. SUPPRESSION
    var incomingIds = dtos.Where(d => d.Id.HasValue).Select(d => d.Id!.Value).ToList();
    var itemsToRemove = profile.Educations.Where(e => !incomingIds.Contains(e.Id)).ToList();
    foreach (var item in itemsToRemove)
    {
        profile.Educations.Remove(item);
    }
        foreach (EducationUpdateDto d in dtos)
        {
            if (d.Id == null)
            {
                Education e = new Education();
                e.Id=Guid.Empty;
                e.ProfileId = profile.Id;
                e.School = d.School;
                e.Degree = d.Degree;
                e.Field = d.Field;
                e.StartDate = d.StartDate.ToUniversalTime();
                e.EndDate = d.EndDate?.ToUniversalTime();
                profile.Educations.Add(e);
            }
            else
            {
                var existingEducation = profile.Educations.FirstOrDefault(e => e.Id == d.Id);
                
                if (existingEducation == null)
                {
                    throw new Exception($"Erreur : La formation avec l'ID {d.Id} n'existe pas dans la base de données.");
                }

                existingEducation.School = d.School;
                existingEducation.Degree = d.Degree;
                existingEducation.Field = d.Field;
                existingEducation.StartDate = d.StartDate.ToUniversalTime();
                existingEducation.EndDate = d.EndDate?.ToUniversalTime();
            }
        }
        
        
    }

    private async Task SyncProjects(Profile profile, List<ProjectUpdateDto> dtos)
    {
        // 1. SUPPRESSION
    var incomingIds = dtos.Where(d => d.Id.HasValue).Select(d => d.Id!.Value).ToList();
    var itemsToRemove = profile.Projects.Where(e => !incomingIds.Contains(e.Id)).ToList();
    foreach (var item in itemsToRemove)
    {
        profile.Projects.Remove(item);
    }
        foreach (ProjectUpdateDto d in dtos)
        {
            if (d.Id == null)
            {
                Project p = new Project();
                p.Id=Guid.Empty;
                p.ProfileId = profile.Id;
                p.Name = d.Name;
                p.Description = d.Description;
                p.Technologies = d.Technologies;
                p.Url = d.Url;
               profile.Projects.Add(p);
            }
            else
            {
                var existingProject = profile.Projects.FirstOrDefault(p => p.Id == d.Id);
                
                if (existingProject == null)
                {
                    throw new Exception($"Erreur : Le projet avec l'ID {d.Id} n'existe pas dans la base de données.");
                }

                existingProject.Name = d.Name;
                existingProject.Description = d.Description;
                existingProject.Technologies = d.Technologies;
                existingProject.Url = d.Url;
            }
        }
        
        
    }

    private async Task SyncSkills(Profile profile, List<SkillUpdateDto> dtos)
    {
        // 1. SUPPRESSION
    var incomingIds = dtos.Where(d => d.Id.HasValue).Select(d => d.Id!.Value).ToList();
    var itemsToRemove = profile.Skills.Where(e => !incomingIds.Contains(e.Id)).ToList();
    foreach (var item in itemsToRemove)
    {
        profile.Skills.Remove(item);
    }
        foreach (SkillUpdateDto d in dtos)
        {
            if (d.Id == null)
            {
                Skill s = new Skill();
                s.Id=Guid.Empty;
                s.ProfileId = profile.Id;
                s.Name = d.Name;
                s.Level = d.Level;
                s.Category = d.Category;
                
                profile.Skills.Add(s);
            }
            else
            {
                var existingSkill = profile.Skills.FirstOrDefault(s => s.Id == d.Id);
                
                if (existingSkill == null)
                {
                    throw new Exception($"Erreur : La compétence avec l'ID {d.Id} n'existe pas dans la base de données.");
                }

                existingSkill.Name = d.Name;
                existingSkill.Level = d.Level;
                existingSkill.Category = d.Category;
            }
        }
        
    }

    private async Task SyncLanguages(Profile profile, List<LanguageUpdateDto> dtos)
    {
        // 1. SUPPRESSION
    var incomingIds = dtos.Where(d => d.Id.HasValue).Select(d => d.Id!.Value).ToList();
    var itemsToRemove = profile.Languages.Where(e => !incomingIds.Contains(e.Id)).ToList();
    foreach (var item in itemsToRemove)
    {
        profile.Languages.Remove(item);
    }
        foreach (LanguageUpdateDto d in dtos)
        {
            if (d.Id == null)
            {
                Language l = new Language();
                l.Id=Guid.Empty;
                l.ProfileId = profile.Id;
                l.Name = d.Name;
                l.Level = d.Level;
                
                profile.Languages.Add(l);
            }
            else
            {
                var existingLanguage = profile.Languages.FirstOrDefault(l => l.Id == d.Id);
                
                if (existingLanguage == null)
                {
                    throw new Exception($"Erreur : La langue avec l'ID {d.Id} n'existe pas dans la base de données.");
                }

                existingLanguage.Name = d.Name;
                existingLanguage.Level = d.Level;
            }
        }
        
        
    }

    private async Task SyncCertifications(Profile profile, List<CertificationUpdateDto> dtos)
    {
        // 1. SUPPRESSION
    var incomingIds = dtos.Where(d => d.Id.HasValue).Select(d => d.Id!.Value).ToList();
    var itemsToRemove = profile.Certifications.Where(e => !incomingIds.Contains(e.Id)).ToList();
    foreach (var item in itemsToRemove)
    {
        profile.Certifications.Remove(item);
    }
        foreach (CertificationUpdateDto d in dtos)
        {
            if (d.Id == null)
            {
                Certification c = new Certification();
                c.Id=Guid.Empty;
                c.ProfileId = profile.Id;
                c.Title = d.Title;
                c.Issuer = d.Issuer;
                c.Year = d.Year;
                
                profile.Certifications.Add(c);
            }
            else
            {
                var existingCertification = profile.Certifications.FirstOrDefault(c => c.Id == d.Id);
                
                if (existingCertification == null)
                {
                    throw new Exception($"Erreur : La certification avec l'ID {d.Id} n'existe pas dans la base de données.");
                }

                existingCertification.Title = d.Title;
                existingCertification.Issuer = d.Issuer;
                existingCertification.Year = d.Year;
            }
        }
        
    }
    // 3. DELETE
    public async Task<bool> DeleteProfileAsync(string keycloakId)
    {
        var user = await _repository.GetUserWithProfileByKeycloakIdAsync(keycloakId);

        if (user == null) return false;

        // En supprimant l'utilisateur, Entity Framework supprimera 
        // automatiquement le Profil lié grâce à la clé étrangère
        _repository.RemoveUser(user);
        await _repository.SaveChangesAsync();

        return true;
    }

    // Méthode utilitaire privée pour éviter de dupliquer le code de mapping
    private ProfileResponseDto MapToResponseDto(User user)
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

            Experiences = profile.Experiences.Select(e => new ExperienceResponseDto { Id = e.Id, Company = e.Company, Position = e.Position, StartDate = e.StartDate, EndDate = e.EndDate, Description = e.Description }).ToList(),
            Educations = profile.Educations.Select(e => new EducationResponseDto { Id = e.Id, School = e.School, Degree = e.Degree, Field = e.Field, StartDate = e.StartDate, EndDate = e.EndDate }).ToList(),
            Projects = profile.Projects.Select(p => new ProjectResponseDto { Id = p.Id, Name = p.Name, Description = p.Description, Technologies = p.Technologies, Url = p.Url }).ToList(),
            Skills = profile.Skills.Select(s => new SkillResponseDto { Id = s.Id, Name = s.Name, Level = s.Level, Category = s.Category }).ToList(),
            Languages = profile.Languages.Select(l => new LanguageResponseDto { Id = l.Id, Name = l.Name, Level = l.Level }).ToList(),
            Certifications = profile.Certifications.Select(c => new CertificationResponseDto { Id = c.Id, Title = c.Title, Issuer = c.Issuer, Year = c.Year }).ToList()
        };
    }
}