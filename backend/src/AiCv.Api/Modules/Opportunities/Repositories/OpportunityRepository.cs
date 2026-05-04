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

    public async Task<List<Opportunity>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.Opportunities
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Opportunity?> GetByIdAndUserIdAsync(Guid id, Guid userId)
    {
        return await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
    }

    public async Task AddAsync(Opportunity opportunity)
    {
        await _context.Opportunities.AddAsync(opportunity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}