using HealthDevice.Data;

namespace HealthDevice.Services;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _dbContext;

    public Repository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
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
}