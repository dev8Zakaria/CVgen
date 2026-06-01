using AiCv.CvService.Modules.Cvs.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiCv.CvService.Data;

public class CvDbContext : DbContext
{
    public CvDbContext(DbContextOptions<CvDbContext> options) : base(options)
    {
    }

    public DbSet<GeneratedCv> GeneratedCvs => Set<GeneratedCv>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GeneratedCv>(entity =>
        {
            entity.HasIndex(cv => cv.KeycloakId);
            entity.HasIndex(cv => cv.OpportunityId);
            entity.Property(cv => cv.ContentJson).HasColumnType("jsonb");
        });
    }
}
