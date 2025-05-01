namespace HealthDevice.Services;

public interface IRepository<T> where T : class
{
    IQueryable<T> Query();
    Task RemoveRange(IEnumerable<T> entities);
    Task Update(T entity);
    Task AddRange(IEnumerable<T> entities);
    Task Add(T entity);
}