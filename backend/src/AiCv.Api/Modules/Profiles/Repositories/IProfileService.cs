using AiCv.Api.Modules.Profiles.Entities;

namespace AiCv.Api.Modules.Profiles.Services;

public interface IProfileService
{
    Task<Profile> GetOrCreateProfileAsync(string keycloakId, string email, string fullName);
}