using HealthDevice.DTO;

namespace HealthDevice.Services;

public interface IRepositoryFactory
{
    IRepository<T> GetRepository<T>() where T : Sensor;
}

public class RepositoryFactory : IRepositoryFactory
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RepositoryFactory(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public IRepository<T> GetRepository<T>() where T : Sensor
    {
        var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetService<IRepository<T>>();
        if (repository == null)
        {
            throw new InvalidOperationException($"No repository found for type {typeof(T).Name}");
        }
        return repository;
    }
}