namespace HealthDevice.Services;

public interface IRepository<T> where T : class
{
    IQueryable<T> Query();
}