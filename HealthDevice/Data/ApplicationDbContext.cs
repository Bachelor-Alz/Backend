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
        public DbSet<Elder> Elders { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<FallInfo> FallInfos { get; set; }
        public DbSet<User> Users { get; set; }
    }
}