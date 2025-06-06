﻿using System.Security.Claims;
using HealthDevice.DTO;
using HealthDevice.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Services;

public interface IUserService
{
    Task<ActionResult<LoginResponseDTO>> HandleLogin(UserLoginDTO userLoginDto, string ipAddress);
    Task<ActionResult> HandleRegister<T>(UserManager<T> userManager, UserRegisterDTO userRegisterDto, T user, string ipAddress) where T : IdentityUser;
    Task<ActionResult<List<ArduinoInfoDTO>>> GetUnusedArduino(Elder elder);
}