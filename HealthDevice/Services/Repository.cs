using HealthDevice.Data;
using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HealthDevice.Services;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<Elder> _elderManager;
    private readonly UserManager<Caregiver> _caregiverManager;

    public Repository(ApplicationDbContext dbContext, UserManager<Elder> userManager, UserManager<Caregiver> caregiverManager)
    {
        _dbContext = dbContext;
        _elderManager = userManager;
        _caregiverManager = caregiverManager;
    }

    public IQueryable<T> Query()
    {
        return _dbContext.Set<T>();
    }
    
    public async Task RemoveRange(IEnumerable<T> entities)
    {
        _dbContext.Set<T>().RemoveRange(entities);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task Update(T entity)
    {
        
        _dbContext.Update(entity);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task AddRange(IEnumerable<T> entities)
    {
        _dbContext.Set<T>().AddRange(entities);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Add(T entity)
    {
        _dbContext.Set<T>().Add(entity);
        await _dbContext.SaveChangesAsync();
    }
    
    public void Attach(T entity)
    {
        EntityEntry<T> entry = _dbContext.Entry(entity);

        // Check if the entity is already tracked
        if (entry.State == EntityState.Detached)
        {
            _dbContext.Set<T>().Attach(entity);
        }

        // Set the entity state to Unchanged for existing entities
        entry.State = EntityState.Unchanged;
    }
}