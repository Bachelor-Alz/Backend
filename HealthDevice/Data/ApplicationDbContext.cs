using HealthDevice.DTO;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Heartrate> Heartrates { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<FallInfo> FallInfos { get; set; }
        public DbSet<Max30102> Max30102Data { get; set; }
        public DbSet<GPS> GpsData { get; set; }
        public DbSet<Elder> Elders { get; set; }
        public DbSet<Caregiver> Caregivers { get; set; }
        public DbSet<Spo2> SpO2s { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable("Locations");
                entity.HasKey(e => e.id);
                entity.Property(e => e.latitude).IsRequired();
                entity.Property(e => e.longitude).IsRequired();
                entity.Property(e => e.timestamp).IsRequired();
            });

            modelBuilder.Entity<Max30102>(entity =>
            {
                entity.ToTable("Max30102Data");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.HeartRate).IsRequired();
                entity.Property(e => e.SpO2).IsRequired();
            });

            modelBuilder.Entity<GPS>(entity =>
            {
                entity.ToTable("GPS_Data");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.Latitude).IsRequired();
                entity.Property(e => e.Longitude).IsRequired();
                entity.Property(e => e.Course).IsRequired();
            });

        }
    }
}