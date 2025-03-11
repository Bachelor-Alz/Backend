using HealthDevice.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Data;

public class UserContext : DbContext
{
    public UserContext()
    {
        
    }
    
    public UserContext(DbContextOptions<UserContext> options)
        : base(options)
    {
    }
    
    public DbSet<Elder> Elders { get; set; }
    public DbSet<Caregiver> Caregivers { get; set; }
    public DbSet<User> Users { get; set; }
}