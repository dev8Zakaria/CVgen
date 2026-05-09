using AiCv.Api.Data;
using AiCv.Api.Modules.Opportunities.Entities;
using AiCv.Api.Modules.Profiles.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiCv.Api.Modules.Cvs.Repositories;

public sealed class CvRepository
{
    private readonly AppDbContext _context;

    public CvRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetUserWithProfileByKeycloakIdAsync(string keycloakId)
    {
        return _context.Users
            .Include(user => user.Profile)
            .FirstOrDefaultAsync(user => user.KeycloakId == keycloakId);
    }

    public Task<JobOffer?> GetAnalyzedOpportunityByIdAndUserIdAsync(Guid opportunityId, Guid userId)
    {
        return _context.JobOffers
            .Include(jobOffer => jobOffer.Analysis)
            .FirstOrDefaultAsync(jobOffer => jobOffer.Id == opportunityId && jobOffer.UserId == userId);
    }
}
