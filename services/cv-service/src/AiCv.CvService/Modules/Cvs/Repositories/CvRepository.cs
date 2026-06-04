using AiCv.CvService.Data;
using AiCv.CvService.Modules.Cvs.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiCv.CvService.Modules.Cvs.Repositories;

public class CvRepository
{
    private readonly CvDbContext _context;

    public CvRepository(CvDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(GeneratedCv cv)
    {
        await _context.GeneratedCvs.AddAsync(cv);
    }

    public async Task AddJobAsync(CvGenerationJob job)
    {
        await _context.CvGenerationJobs.AddAsync(job);
    }

    public Task<List<GeneratedCv>> GetAllByUserAsync(string keycloakId)
    {
        return _context.GeneratedCvs
            .Where(cv => cv.KeycloakId == keycloakId)
            .OrderByDescending(cv => cv.CreatedAt)
            .ToListAsync();
    }

    public Task<GeneratedCv?> GetByIdAndUserAsync(Guid id, string keycloakId)
    {
        return _context.GeneratedCvs.FirstOrDefaultAsync(cv => cv.Id == id && cv.KeycloakId == keycloakId);
    }

    public Task<CvGenerationJob?> GetJobByIdAndUserAsync(Guid id, string keycloakId)
    {
        return _context.CvGenerationJobs.FirstOrDefaultAsync(job => job.Id == id && job.KeycloakId == keycloakId);
    }

    public Task<CvGenerationJob?> GetJobByIdAsync(Guid id)
    {
        return _context.CvGenerationJobs.FirstOrDefaultAsync(job => job.Id == id);
    }

    public void Remove(GeneratedCv cv)
    {
        _context.GeneratedCvs.Remove(cv);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
