using HealthDevice.DTO;
using HealthDevice.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Heartrate> Heartrate { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<FallInfo> FallInfo { get; set; }
        public DbSet<GPSData> GPSData { get; set; }
        public DbSet<Elder> Elder { get; set; }
        public DbSet<Caregiver> Caregiver { get; set; }
        public DbSet<Spo2> SpO2 { get; set; }
        public DbSet<Steps> Steps { get; set; }
        public DbSet<DistanceInfo> Distance { get; set; }
        public DbSet<Perimeter> Perimeter { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }
        public DbSet<Arduino> Arduino { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure one-to-many relationship for assigned elders
            modelBuilder.Entity<Elder>()
                .HasOne(e => e.Caregiver)
                .WithMany(c => c.Elders)
                .HasForeignKey(e => e.CaregiverId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure one-to-many relationship for invited elders
            modelBuilder.Entity<Elder>()
                .HasOne(e => e.InvitedCaregiver)
                .WithMany(c => c.Invites)
                .HasForeignKey(e => e.InvitedCaregiverId)
                .OnDelete(DeleteBehavior.SetNull);
    
            // Configure one-to-one relationship for Arduino and Elder
            modelBuilder.Entity<Elder>()
                .HasOne(a => a.Arduino)
                .WithOne(e => e.elder)
                .HasForeignKey<Elder>(e => e.MacAddress)
                .OnDelete(DeleteBehavior.SetNull);
    
            // Configure the base Sensor entity
            modelBuilder.Entity<Sensor>()
                .HasOne(a => a.Arduino)
                .WithMany(e => e.Sensors)
                .HasForeignKey(s => s.MacAddress);

            // Use TPT (Table-per-Type) inheritance strategy
            modelBuilder.Entity<Sensor>().ToTable("Sensors");
            modelBuilder.Entity<Heartrate>().ToTable("Heartrates");
            modelBuilder.Entity<GPSData>().ToTable("GPSData");
            modelBuilder.Entity<FallInfo>().ToTable("FallInfo");
            modelBuilder.Entity<DistanceInfo>().ToTable("DistanceInfo");
            modelBuilder.Entity<Spo2>().ToTable("SpO2");
            modelBuilder.Entity<Steps>().ToTable("Steps");
        }
    }
}