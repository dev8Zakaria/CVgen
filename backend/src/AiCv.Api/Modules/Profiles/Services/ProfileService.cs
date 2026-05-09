using AiCv.Api.Modules.Profiles.DTOs;
using AiCv.Api.Modules.Profiles.Entities;
using AiCv.Api.Modules.Profiles.Repositories;

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
    public async Task<ProfileResponseDto?> UpdateProfileAsync(string keycloakId, ProfileUpdateRequestDto request)
    {
        var user = await _repository.GetUserWithProfileByKeycloakIdAsync(keycloakId);

        if (user == null || user.Profile == null) return null;

        // Mise à jour des champs
        user.Profile.Title = request.Title;
        user.Profile.Summary = request.Summary;
        user.Profile.Phone = request.Phone;
        user.Profile.Location = request.Location;
        user.Profile.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return MapToResponseDto(user);
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
        return new ProfileResponseDto
        {
            Id = user.Profile!.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            Title = user.Profile.Title,
            Summary = user.Profile.Summary,
            Phone = user.Profile.Phone,
            Location = user.Profile.Location,
            UpdatedAt = user.Profile.UpdatedAt
        };
    }
}