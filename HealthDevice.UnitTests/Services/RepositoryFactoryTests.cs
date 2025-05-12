using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using HealthDevice.Models;

public class RepositoryFactoryTest
{
    private readonly Mock<IServiceProvider> mockServiceProvider;
    private readonly IRepositoryFactory repositoryFactory;

    public RepositoryFactoryTest()
    {
        mockServiceProvider = new Mock<IServiceProvider>();

        var mockScope = new Mock<IServiceScope>();
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

        repositoryFactory = new RepositoryFactory(mockScopeFactory.Object);
    }

    [Fact]
    public void GetRepositoryReturnsValidServices()
    {
        var mockRepo = new Mock<IRepository<Max30102>>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IRepository<Max30102>)))
            .Returns(mockRepo.Object);

        var repo = repositoryFactory.GetRepository<Max30102>();
        Assert.NotNull(repo);
        Assert.Equal(mockRepo.Object, repo);
    }

    [Fact]
    public void GetRepositoryThrowsExceptionWhenNoRepositoryFound()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            repositoryFactory.GetRepository<Sensor>());

        Assert.Equal("No repository found for type Sensor", exception.Message);
    }
}