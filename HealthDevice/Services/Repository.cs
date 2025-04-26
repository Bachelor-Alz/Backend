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
    
    public void RemoveRange(IEnumerable<T> entities)
    {
        _dbContext.Set<T>().RemoveRange(entities);
    }
    
    public void Update(T entity)
    {
        _dbContext.Set<T>().Update(entity);
    }
    
    public void AddRange(IEnumerable<T> entities)
    {
        _dbContext.Set<T>().AddRange(entities);
    }

    public void Add(T entity)
    {
        _dbContext.Set<T>().Add(entity);
    }
}