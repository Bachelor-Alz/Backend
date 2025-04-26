namespace HealthDevice.Services;

public interface IRepository<T> where T : class
{
    IQueryable<T> Query();
    void RemoveRange(IEnumerable<T> entities);
    void Update(T entity);
    void AddRange(IEnumerable<T> entities);
    void Add(T entity);
}