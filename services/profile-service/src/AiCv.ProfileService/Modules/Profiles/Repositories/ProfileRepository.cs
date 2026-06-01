using AiCv.ProfileService.Data;
using AiCv.ProfileService.Modules.Profiles.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiCv.ProfileService.Modules.Profiles.Repositories;

public class ProfileRepository
{
    private readonly ProfileDbContext _context;

    public ProfileRepository(ProfileDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetUserWithProfileByKeycloakIdAsync(string keycloakId)
    {
        return _context.Users
            .Include(user => user.Profile)
                .ThenInclude(profile => profile!.Experiences)
            .Include(user => user.Profile)
                .ThenInclude(profile => profile!.Educations)
            .Include(user => user.Profile)
                .ThenInclude(profile => profile!.Projects)
            .Include(user => user.Profile)
                .ThenInclude(profile => profile!.Skills)
            .Include(user => user.Profile)
                .ThenInclude(profile => profile!.Languages)
            .Include(user => user.Profile)
                .ThenInclude(profile => profile!.Certifications)
            .FirstOrDefaultAsync(user => user.KeycloakId == keycloakId);
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public void RemoveUser(User user)
    {
        _context.Users.Remove(user);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
