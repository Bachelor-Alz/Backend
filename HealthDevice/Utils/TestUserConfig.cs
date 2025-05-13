using HealthDevice.Controllers;
using HealthDevice.DTO;
using HealthDevice.Models;
using HealthDevice.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Utils;

public static class TestUserConfig
{
    public static async Task MakeTestUserAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;

        var userController = scopedServices.GetRequiredService<UserController>();

        UserRegisterDTO elder = new UserRegisterDTO
        {
            Name = "Test",
            Email = "Test@Test.dk",
            Password = "Test1234!",
            Latitude = 55.6761,
            Longitude = 12.5683,
            Role = Roles.Elder
        };
        var result = await userController.Register(elder);

        if (result is not OkObjectResult)
        {
            return;
        }
        IRepository<Elder> elderRepository = scopedServices.GetRequiredService<IRepository<Elder>>();
        Elder? elderEntity = await elderRepository.Query().FirstOrDefaultAsync(e => e.Email == elder.Email);
        if (elderEntity == null) return;

        elderEntity.MacAddress = "00:00:00:00:00:00";
        await elderRepository.Update(elderEntity);

        var testController = scopedServices.GetRequiredService<TestController>();
        await testController.GenerateFakeData(elderEntity.Id);
    }
}