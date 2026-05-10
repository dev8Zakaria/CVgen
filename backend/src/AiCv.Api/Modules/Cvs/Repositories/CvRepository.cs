using AiCv.Api.Data;
using AiCv.Api.Modules.Cvs.Entities;

namespace AiCv.Api.Modules.Cvs.Repositories;

public class CvRepository
{
    private readonly AppDbContext _context;

    public CvRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddCvAsync(Cv cv)
    {
        await _context.Cvs.AddAsync(cv);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}