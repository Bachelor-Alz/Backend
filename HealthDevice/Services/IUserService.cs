using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Services;

public interface IUserService
{
    Task<ActionResult<LoginResponseDTO>> HandleLogin(UserLoginDTO userLoginDto, string ipAddress);
    Task<ActionResult> HandleRegister<T>(UserManager<T> userManager, UserRegisterDTO userRegisterDto, T user, string ipAddress) where T : IdentityUser;
    string GenerateJwt<T>(T user, string role) where T : IdentityUser;
}