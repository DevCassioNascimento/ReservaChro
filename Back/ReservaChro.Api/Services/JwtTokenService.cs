using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ReservaChro.Domain.Entities;

namespace ReservaChro.Api.Services;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) Generate(User user);
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public (string Token, DateTime ExpiresAtUtc) Generate(User user)
    {
        var jwt = _config.GetSection("JwtSettings");

        var key = jwt["Key"] ?? throw new InvalidOperationException("JwtSettings:Key não configurado.");
        var issuer = jwt["Issuer"] ?? throw new InvalidOperationException("JwtSettings:Issuer não configurado.");
        var audience = jwt["Audience"] ?? throw new InvalidOperationException("JwtSettings:Audience não configurado.");

        var expiresMinutesRaw = jwt["ExpiresMinutes"] ?? jwt["ExpiresInMinutes"];
        var expiresMinutes = int.TryParse(expiresMinutesRaw, out var m) ? m : 120;

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name ?? string.Empty),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        // Se você usa isolamento por escola
        if (user.SchoolId is not null)
            claims.Add(new Claim("schoolId", user.SchoolId.ToString()!));

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
