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
    }
}