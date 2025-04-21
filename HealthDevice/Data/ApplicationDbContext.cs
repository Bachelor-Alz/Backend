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
        
        public DbSet<Heartrate> Heartrate { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<FallInfo> FallInfo { get; set; }
        public DbSet<Max30102> MAX30102Data { get; set; }
        public DbSet<GPS> GPSData { get; set; }
        public DbSet<Elder> Elder { get; set; }
        public DbSet<Caregiver> Caregiver { get; set; }
        public DbSet<Spo2> SpO2 { get; set; }
        public DbSet<Steps> Steps { get; set; }
        public DbSet<Kilometer> Distance { get; set; }
        public DbSet<Perimeter> Perimeter { get; set; }
    }
}