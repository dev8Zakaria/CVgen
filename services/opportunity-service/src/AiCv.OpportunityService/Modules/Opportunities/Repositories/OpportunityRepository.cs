using AiCv.OpportunityService.Data;
using AiCv.OpportunityService.Modules.Opportunities.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiCv.OpportunityService.Modules.Opportunities.Repositories;

public class OpportunityRepository
{
    private readonly OpportunityDbContext _context;

    public OpportunityRepository(OpportunityDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> GetOrCreateUserIdByKeycloakIdAsync(string keycloakId)
    {
        var existingUserId = await _context.Users
            .Where(user => user.KeycloakId == keycloakId)
            .Select(user => (Guid?)user.Id)
            .FirstOrDefaultAsync();

        if (existingUserId.HasValue)
        {
            return existingUserId.Value;
        }

        var user = new OpportunityUser
        {
            KeycloakId = keycloakId
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return user.Id;
    }

    public Task<List<JobOffer>> GetAllByUserIdAsync(Guid userId)
    {
        return _context.JobOffers
            .Where(jobOffer => jobOffer.UserId == userId)
            .OrderByDescending(jobOffer => jobOffer.CreatedAt)
            .ToListAsync();
    }

    public Task<JobOffer?> GetByIdAndUserIdAsync(Guid id, Guid userId)
    {
        return _context.JobOffers
            .Include(jobOffer => jobOffer.Analysis)
            .FirstOrDefaultAsync(jobOffer => jobOffer.Id == id && jobOffer.UserId == userId);
    }

    public async Task AddAsync(JobOffer jobOffer)
    {
        await _context.JobOffers.AddAsync(jobOffer);
    }

    public void Remove(JobOffer jobOffer)
    {
        _context.JobOffers.Remove(jobOffer);
    }

    public void RemoveAnalysis(JobOfferAnalysis analysis)
    {
        _context.JobOfferAnalyses.Remove(analysis);
    }

    public async Task AddAnalysisAsync(JobOfferAnalysis analysis)
    {
        await _context.JobOfferAnalyses.AddAsync(analysis);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
