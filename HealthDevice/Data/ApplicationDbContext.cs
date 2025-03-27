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
        public DbSet<IMU> Mpu6050Data { get; set; }
        public DbSet<GPS> GpsData { get; set; }
        public DbSet<Elder> Elders { get; set; }
        public DbSet<Caregiver> Caregivers { get; set; }

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

            modelBuilder.Entity<FallInfo>(entity =>
            {
                entity.ToTable("FallInfos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.timestamp).IsRequired();
                entity.Property(e => e.status).IsRequired();
                entity.HasOne(e => e.location)
                    .WithMany()
                    .HasForeignKey(e => e.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Max30102>(entity =>
            {
                entity.ToTable("Max30102Data");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.BPM).IsRequired();
                entity.Property(e => e.SpO2).IsRequired();
            });

            modelBuilder.Entity<IMU>(entity =>
            {
                entity.ToTable("IMU_Data");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.AccelerationX).IsRequired();
                entity.Property(e => e.AccelerationY).IsRequired();
                entity.Property(e => e.AccelerationZ).IsRequired();
                entity.Property(e => e.GyroscopeX).IsRequired();
                entity.Property(e => e.GyroscopeY).IsRequired();
                entity.Property(e => e.GyroscopeZ).IsRequired();
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