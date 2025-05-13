using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using HealthDevice.Models;
using HealthDevice.Data;
using HealthDevice.DTO;
using HealthDevice.Utils;

public class TokenService
{
    private readonly string _jwtSecret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _jwtLifespanMinutes;
    private readonly int _refreshTokenLifespanDays;
    private readonly ApplicationDbContext _dbContext;

    public TokenService(
        JwtSettings jwtSettings,
        ApplicationDbContext dbContext)
    {
        _jwtSecret = jwtSettings.Secret;
        _issuer = jwtSettings.Issuer;
        _audience = jwtSettings.Audience;
        _jwtLifespanMinutes = jwtSettings.AccessTokenLifetimeMinutes;
        _refreshTokenLifespanDays = jwtSettings.RefreshTokenLifetimeDays;
        _dbContext = dbContext;
    }

    public string GenerateAccessToken<T>(T user, string role) where T : IdentityUser
    {
        if (user.Email == null) return string.Empty;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtLifespanMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<RefreshTokenResult> IssueRefreshTokenAsync(string userEmail, string? createdByIp = null)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenHash = HashToken(rawToken);

        var refreshToken = new RefreshToken
        {
            Email = userEmail,
            TokenHash = tokenHash,
            Expiration = DateTime.UtcNow.AddDays(_refreshTokenLifespanDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = createdByIp,
        };

        // Revoke all previous active tokens for this user
        var tokensToRevoke = await _dbContext.RefreshToken
            .Where(rt => rt.Email == userEmail && !rt.IsRevoked && rt.Expiration > DateTime.UtcNow)
            .ToListAsync();

        foreach (var rt in tokensToRevoke)
        {
            rt.IsRevoked = true;
            rt.RevokedAt = DateTime.UtcNow;
            rt.RevokedByIp = createdByIp;
        }

        _dbContext.RefreshToken.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        return new RefreshTokenResult
        {
            Token = rawToken,
        };
    }


    public async Task<RefreshTokenResult?> RotateRefreshTokenAsync(string oldRawToken, string? createdByIp = null)
    {
        var oldTokenHash = HashToken(oldRawToken);
        var oldToken = await _dbContext.RefreshToken
            .FirstOrDefaultAsync(rt => rt.TokenHash == oldTokenHash);

        if (oldToken == null || oldToken.IsRevoked || oldToken.Expiration < DateTime.UtcNow)
            return null;

        var newRawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var newTokenHash = HashToken(newRawToken);

        var newToken = new RefreshToken
        {
            Email = oldToken.Email,
            TokenHash = newTokenHash,
            Expiration = DateTime.UtcNow.AddDays(_refreshTokenLifespanDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = createdByIp
        };

        oldToken.IsRevoked = true;
        oldToken.RevokedAt = DateTime.UtcNow;
        oldToken.RevokedByIp = createdByIp;
        oldToken.ReplacedByTokenHash = newTokenHash;

        _dbContext.RefreshToken.Add(newToken);
        await _dbContext.SaveChangesAsync();

        return new RefreshTokenResult
        {
            Token = newRawToken
        };
    }


    public async Task<bool> RevokeRefreshTokenAsync(string rawToken, string? revokedByIp = null)
    {
        var tokenHash = HashToken(rawToken);
        var storedToken = await _dbContext.RefreshToken
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (storedToken == null || storedToken.IsRevoked || storedToken.IsExpired)
            return false;

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = revokedByIp;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ValidateRefreshTokenAsync(string rawToken)
    {
        var tokenHash = HashToken(rawToken);
        var storedToken = await _dbContext.RefreshToken
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        return storedToken != null && !storedToken.IsRevoked && !storedToken.IsExpired;
    }

    private string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
