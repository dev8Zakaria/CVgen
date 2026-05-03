using Microsoft.EntityFrameworkCore;
using AiCv.Api.Data;
using AiCv.Api.Modules.Profiles.Entities;

namespace AiCv.Api.Modules.Profiles.Services;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _context;

    public ProfileService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Profile> GetOrCreateProfileAsync(string keycloakId, string email, string fullName)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);

        // Auto-création si première connexion
        if (user == null)
        {
            user = new User
            {
                KeycloakId = keycloakId,
                Email = email,
                FullName = fullName,
                Profile = new Profile 
                { 
                    Title = "Nouveau Profil",
                    Summary = "Généré automatiquement suite à l'inscription." 
                }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        return user.Profile!;
    }
}