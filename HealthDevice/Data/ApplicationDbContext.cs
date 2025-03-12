using Microsoft.EntityFrameworkCore;
using HealthDevice.Models;

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
        public DbSet<Max30102> Max30102Datas { get; set; }
        public DbSet<MPU6050> MPU6050Datas { get; set; }
        public DbSet<Neo_6m> Neo_6mDatas { get; set; }
        public DbSet<Elder> Elders { get; set; }
        public DbSet<Caregiver> Caregivers { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Location>(entity =>
    {
        entity.ToTable("Locations");
        entity.HasKey(e => e.id);
        entity.Property(e => e.latitude).IsRequired();
        entity.Property(e => e.longitude).IsRequired();
        entity.Property(e => e.altitude).IsRequired();
        entity.Property(e => e.timestamp).IsRequired();
    });

    modelBuilder.Entity<FallInfo>(entity =>
    {
        entity.ToTable("FallInfos");
        entity.HasKey(e => e.id);
        entity.Property(e => e.timestamp).IsRequired();
        entity.Property(e => e.status).IsRequired();
        entity.HasOne(e => e.location)
            .WithMany()
            .HasForeignKey(e => e.id)
            .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<Max30102>(entity =>
    {
        entity.ToTable("Max30102Datas");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Red).IsRequired();
        entity.Property(e => e.Infrared).IsRequired();
        entity.Property(e => e.HeartRate);
        entity.Property(e => e.SpO2);
        entity.Property(e => e.Timestamp).IsRequired();
    });

    modelBuilder.Entity<MPU6050>(entity =>
    {
        entity.ToTable("MPU6050Datas");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.AccelerationX).IsRequired();
        entity.Property(e => e.AccelerationY).IsRequired();
        entity.Property(e => e.AccelerationZ).IsRequired();
        entity.Property(e => e.GyroscopeX).IsRequired();
        entity.Property(e => e.GyroscopeY).IsRequired();
        entity.Property(e => e.GyroscopeZ).IsRequired();
        entity.Property(e => e.Timestamp).IsRequired();
        entity.Property(e => e.temperature).IsRequired();
    });

    modelBuilder.Entity<Neo_6m>(entity =>
    {
        entity.ToTable("Neo_6mDatas");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.UtcTime).IsRequired();
        entity.Property(e => e.Status).IsRequired();
        entity.Property(e => e.Latitude).IsRequired();
        entity.Property(e => e.LatitudeDirection).IsRequired();
        entity.Property(e => e.Longitude).IsRequired();
        entity.Property(e => e.LongitudeDirection).IsRequired();
        entity.Property(e => e.SpeedKnots).IsRequired();
        entity.Property(e => e.Course).IsRequired();
        entity.Property(e => e.Date).IsRequired();
        entity.Property(e => e.MagneticVariation);
        entity.Property(e => e.MagneticDirection);
        entity.Property(e => e.Checksum).IsRequired();
    });

    modelBuilder.Entity<Location>().ToTable("Locations", t => t.ExcludeFromMigrations());
    modelBuilder.Entity<FallInfo>().ToTable("FallInfos", t => t.ExcludeFromMigrations());
    modelBuilder.Entity<Max30102>().ToTable("Max30102Datas", t => t.ExcludeFromMigrations());
    modelBuilder.Entity<MPU6050>().ToTable("MPU6050Datas", t => t.ExcludeFromMigrations());
    modelBuilder.Entity<Neo_6m>().ToTable("Neo_6mDatas", t => t.ExcludeFromMigrations());
    modelBuilder.Entity<User>().ToTable("Users", t => t.ExcludeFromMigrations());
    modelBuilder.Entity<Elder>().ToTable("Elders", t => t.ExcludeFromMigrations());
    modelBuilder.Entity<Caregiver>().ToTable("Caregivers", t => t.ExcludeFromMigrations());
    modelBuilder.Entity<Heartrate>().ToTable("Heartrates", t => t.ExcludeFromMigrations());
}
    }
} 