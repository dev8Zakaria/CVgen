using AiCv.CvService.Modules.Cvs.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiCv.CvService.Data;

public class CvDbContext : DbContext
{
    public CvDbContext(DbContextOptions<CvDbContext> options) : base(options)
    {
    }

    public DbSet<GeneratedCv> GeneratedCvs => Set<GeneratedCv>();
    public DbSet<CvGenerationJob> CvGenerationJobs => Set<CvGenerationJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GeneratedCv>(entity =>
        {
            entity.HasIndex(cv => cv.KeycloakId);
            entity.HasIndex(cv => cv.OpportunityId);
            entity.Property(cv => cv.ContentJson).HasColumnType("jsonb");
        });

        modelBuilder.Entity<CvGenerationJob>(entity =>
        {
            entity.HasIndex(job => job.KeycloakId);
            entity.HasIndex(job => job.Status);
            entity.HasIndex(job => job.GeneratedCvId);
            entity.Property(job => job.Status).HasMaxLength(32);
            entity.Property(job => job.KeycloakId).HasMaxLength(128);
            entity.Property(job => job.ProfileSnapshotJson).HasColumnType("jsonb");
            entity.Property(job => job.OpportunitySnapshotJson).HasColumnType("jsonb");
        });
    }
}
