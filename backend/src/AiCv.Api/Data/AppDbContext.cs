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
