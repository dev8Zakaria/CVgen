using AiCv.ProfileService.Modules.Profiles.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiCv.ProfileService.Data;

public class ProfileDbContext : DbContext
{
    public ProfileDbContext(DbContextOptions<ProfileDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Experience> Experiences => Set<Experience>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Certification> Certifications => Set<Certification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(user => user.KeycloakId).IsUnique();
            entity.HasIndex(user => user.Email);

            entity.HasOne(user => user.Profile)
                .WithOne(profile => profile.User)
                .HasForeignKey<Profile>(profile => profile.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasMany(profile => profile.Experiences)
                .WithOne(experience => experience.Profile)
                .HasForeignKey(experience => experience.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(profile => profile.Educations)
                .WithOne(education => education.Profile)
                .HasForeignKey(education => education.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(profile => profile.Projects)
                .WithOne(project => project.Profile)
                .HasForeignKey(project => project.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(profile => profile.Skills)
                .WithOne(skill => skill.Profile)
                .HasForeignKey(skill => skill.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(profile => profile.Languages)
                .WithOne(language => language.Profile)
                .HasForeignKey(language => language.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(profile => profile.Certifications)
                .WithOne(certification => certification.Profile)
                .HasForeignKey(certification => certification.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
