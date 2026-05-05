using Microsoft.EntityFrameworkCore;
using AiCv.Api.Data;
using AiCv.Api.Modules.Opportunities.Entities;

namespace AiCv.Api.Modules.Opportunities.Repositories;

public class OpportunityRepository
{
    private readonly AppDbContext _context;

    public OpportunityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> GetUserIdByKeycloakIdAsync(string keycloakId)
    {
        return await _context.Users
            .Where(user => user.KeycloakId == keycloakId)
            .Select(user => (Guid?)user.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<JobOffer>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.JobOffers
            .Where(jobOffer => jobOffer.UserId == userId)
            .OrderByDescending(jobOffer => jobOffer.CreatedAt)
            .ToListAsync();
    }

    public async Task<JobOffer?> GetByIdAndUserIdAsync(Guid id, Guid userId)
    {
        return await _context.JobOffers
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

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
