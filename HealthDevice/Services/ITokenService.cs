using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;

namespace HealthDevice.Services;

public interface ITokenService
{
    string GenerateAccessToken<T>(T user, string role) where T : IdentityUser;
    Task<RefreshTokenResult> IssueRefreshTokenAsync(string userId, string? createdByIp = null);
    Task<RefreshTokenResult?> RotateRefreshTokenAsync(string oldRawToken, string? createdByIp = null);
    Task<bool> RevokeRefreshTokenAsync(string rawToken, string? revokedByIp = null);
    Task<bool> ValidateRefreshTokenAsync(string rawToken);
}