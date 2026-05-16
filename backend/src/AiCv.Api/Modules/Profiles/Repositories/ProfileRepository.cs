using AiCv.Api.Data;
using AiCv.Api.Modules.Profiles.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiCv.Api.Modules.Profiles.Repositories;

public class ProfileRepository
{
    private readonly AppDbContext _context;

    public ProfileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserWithProfileByKeycloakIdAsync(string keycloakId)
    {
        return await _context.Users
            .Include(u => u.Profile)
                .ThenInclude(p => p.Experiences)
            .Include(u => u.Profile)
                .ThenInclude(p => p.Educations)
            .Include(u => u.Profile)
                .ThenInclude(p => p.Projects)
            .Include(u => u.Profile)
                .ThenInclude(p => p.Skills)
            .Include(u => u.Profile)
                .ThenInclude(p => p.Languages)
            .Include(u => u.Profile)
                .ThenInclude(p => p.Certifications)
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);
    }
    

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public void RemoveUser(User user)
    {
        _context.Users.Remove(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}