using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Services;

public interface IUserService
{
    Task<ActionResult<LoginResponseDTO>> HandleLogin(UserLoginDTO userLoginDto, HttpContext httpContext);
    Task<ActionResult> HandleRegister<T>(UserManager<T> userManager, UserRegisterDTO userRegisterDto, T user, HttpContext httpContext) where T : IdentityUser;
}