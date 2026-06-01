using AiCv.OpportunityService.Modules.Opportunities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCv.OpportunityService.Data;

public class OpportunityDbContext : DbContext
{
    public OpportunityDbContext(DbContextOptions<OpportunityDbContext> options) : base(options)
    {
    }

    public DbSet<OpportunityUser> Users => Set<OpportunityUser>();
    public DbSet<JobOffer> JobOffers => Set<JobOffer>();
    public DbSet<JobOfferAnalysis> JobOfferAnalyses => Set<JobOfferAnalysis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OpportunityUser>(entity =>
        {
            entity.HasIndex(user => user.KeycloakId).IsUnique();
            entity.HasMany(user => user.JobOffers)
                .WithOne(jobOffer => jobOffer.User)
                .HasForeignKey(jobOffer => jobOffer.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JobOffer>(entity =>
        {
            entity.HasOne(jobOffer => jobOffer.Analysis)
                .WithOne(analysis => analysis.JobOffer)
                .HasForeignKey<JobOfferAnalysis>(analysis => analysis.JobOfferId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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
        });
    }

    private static void ConfigureStringList(PropertyBuilder<List<string>> propertyBuilder)
    {
        propertyBuilder.HasConversion(
            values => string.Join('\n', values),
            values => values.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList());

        propertyBuilder.Metadata.SetValueComparer(new ValueComparer<List<string>>(
            (left, right) => (left ?? new List<string>()).SequenceEqual(right ?? new List<string>()),
            values => (values ?? new List<string>()).Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            values => values == null ? new List<string>() : values.ToList()));
    }
}
