using HealthDevice.Controllers;
using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;


namespace HealthDevice.Utils;

public static class TestUserConfig
{
    //Want to make an elder user than also gets an macadress and calls FakeData from Testcontroller
        
    public static async Task MakeTestUserAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;

        // Resolve UserController
        var userController = scopedServices.GetRequiredService<UserController>();

        // Create a new elder user
        var elder = new UserRegisterDTO
        {
            Name = "Test",
            Email = "Test@Test.dk",
            Password = "Test1234!",
            latitude = 55.6761,
            longitude = 12.5683,
            Role = Roles.Elder,
        };
        
        // Register the user
       var result = await userController.Register(elder);

        if (result is not OkObjectResult)
        {
            return;
        }
        // Resolve Elder repository and set the MAC address
        var elderRepository = scopedServices.GetRequiredService<IRepository<Elder>>();
        var elderEntity = await elderRepository.Query().FirstOrDefaultAsync(e => e.Email == elder.Email);
        if (elderEntity == null) return;

        elderEntity.MacAddress = "00:00:00:00:00:00";
        await elderRepository.Update(elderEntity);

        // Generate fake data using TestController
        var testController = scopedServices.GetRequiredService<TestController>();
        await testController.GenerateFakeData(elder.Email);
    }
}