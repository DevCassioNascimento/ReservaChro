using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaChro.Api.Services;
using ReservaChro.Application.DTOs.Auth;
using ReservaChro.Infrastructure.Data;

namespace ReservaChro.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IJwtTokenService _jwt;

    public AuthController(AppDbContext dbContext, IJwtTokenService jwt)
    {
        _dbContext = dbContext;
        _jwt = jwt;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto? request)
    {
        // ✅ proteção contra body null (evita 500 em chamadas erradas)
        if (request is null)
            return BadRequest("Request body is required.");

        // ✅ Normaliza entrada
        var username = (request.Username ?? string.Empty).Trim();
        var password = request.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return BadRequest("Username and password are required.");

        // ✅ Busca case-insensitive e tolerante a e-mail salvo com maiúsculas
        // (Evita falhas de login por diferença de caixa)
        var normalized = username.ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized);

        if (user is null)
            return Unauthorized("Invalid credentials.");

        // TEMPORÁRIO (sem hash)
        if (user.PasswordHash != password)
            return Unauthorized("Invalid credentials.");

        var (token, expiresAtUtc) = _jwt.Generate(user);

        var response = new LoginResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            SchoolId = user.SchoolId,
            Token = token,
            ExpiresAtUtc = expiresAtUtc
        };

        return Ok(response);
    }
}
