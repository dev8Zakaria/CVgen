using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AiCv.Api.Modules.Profiles.Entities;
using AiCv.Api.Modules.Opportunities.Entities;
using AiCv.Api.Modules.Cvs.Entities;

namespace AiCv.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<Experience> Experiences { get; set; }
    public DbSet<Education> Educations { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Certification> Certifications { get; set; }
    public DbSet<Cv> Cvs { get; set; }
    public DbSet<JobOffer> JobOffers { get; set; }
    public DbSet<JobOfferAnalysis> JobOfferAnalyses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<JobOfferAnalysis>(entity =>
        {
            ConfigureStringList(entity.Property(analysis => analysis.ExtractedSkills));
            ConfigureStringList(entity.Property(analysis => analysis.ExtractedKeywords));
            ConfigureStringList(entity.Property(analysis => analysis.ExtractedResponsibilities));
            ConfigureStringList(entity.Property(analysis => analysis.DetectedTechnologies));
            ConfigureStringList(entity.Property(analysis => analysis.MustHaveRequirements));
            ConfigureStringList(entity.Property(analysis => analysis.NiceToHaveRequirements));
            ConfigureStringList(entity.Property(analysis => analysis.CvFocusPoints));
            ConfigureStringList(entity.Property(analysis => analysis.CandidateRisks));
            modelBuilder.Entity<Profile>(entity =>
            {
                entity.HasMany(p => p.Experiences)
                    .WithOne(e => e.Profile)
                    .HasForeignKey(e => e.ProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
            
                entity.HasMany(p => p.Educations)
                    .WithOne(e => e.Profile)
                    .HasForeignKey(e => e.ProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
            
                entity.HasMany(p => p.Projects)
                    .WithOne(e => e.Profile)
                    .HasForeignKey(e => e.ProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
            
                entity.HasMany(p => p.Skills)
                    .WithOne(e => e.Profile)
                    .HasForeignKey(e => e.ProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
            
                entity.HasMany(p => p.Languages)
                    .WithOne(e => e.Profile)
                    .HasForeignKey(e => e.ProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
            
                entity.HasMany(p => p.Certifications)
                    .WithOne(e => e.Profile)
                    .HasForeignKey(e => e.ProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            entity.HasOne(analysis => analysis.JobOffer)
                .WithOne(jobOffer => jobOffer.Analysis)
                .HasForeignKey<JobOfferAnalysis>(analysis => analysis.JobOfferId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureStringList(PropertyBuilder<List<string>> propertyBuilder)
    {
        propertyBuilder.HasConversion(
            values => string.Join(',', values),
            values => values.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

        propertyBuilder.Metadata.SetValueComparer(new ValueComparer<List<string>>(
            (left, right) => (left ?? new List<string>()).SequenceEqual(right ?? new List<string>()),
            values => (values ?? new List<string>()).Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            values => values == null ? new List<string>() : values.ToList()));
    }
}
