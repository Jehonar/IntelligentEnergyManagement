using IntelligentEnergy.API.Models;
using Microsoft.EntityFrameworkCore;

namespace IntelligentEnergy.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<EnergyReading> EnergyReadings => Set<EnergyReading>();
    public DbSet<Prediction> Predictions => Set<Prediction>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EnergyReading>(entity =>
        {
            entity.Property(e => e.EnergyConsumption).HasColumnType("decimal(10,4)");
            entity.Property(e => e.Temperature).HasColumnType("decimal(5,2)");
            entity.Property(e => e.DeviceName).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.ReadingDate);
            entity.HasIndex(e => e.DeviceName);
        });

        modelBuilder.Entity<Prediction>(entity =>
        {
            entity.Property(e => e.PredictedConsumption).HasColumnType("decimal(10,4)");
        });

        modelBuilder.Entity<Recommendation>(entity =>
        {
            entity.Property(e => e.Message).HasMaxLength(500).IsRequired();
            entity.Property(e => e.RecommendationType).HasMaxLength(50).IsRequired();
            entity.HasOne(r => r.Prediction)
                  .WithMany()
                  .HasForeignKey(r => r.PredictionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
