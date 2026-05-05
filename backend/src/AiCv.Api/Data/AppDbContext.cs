using Microsoft.EntityFrameworkCore;
using AiCv.Api.Modules.Profiles.Entities;
using AiCv.Api.Modules.Opportunities.Entities;

namespace AiCv.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<Opportunity> Opportunities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Opportunity>(entity =>
        {
            entity.Property(o => o.ExtractedSkills)
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                  );

            entity.Property(o => o.ExtractedKeywords)
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                  );
        });
    }
}