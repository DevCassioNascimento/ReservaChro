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
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username and password are required.");

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Username.ToLower());

        if (user is null)
            return Unauthorized("Invalid credentials.");

        // TEMPORÁRIO (sem hash)
        if (user.PasswordHash != request.Password)
            return Unauthorized("Invalid credentials.");

        var (token, expiresAtUtc) = _jwt.Generate(user);

        var response = new LoginResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Username = user.Email,
            Role = user.Role.ToString(),
            SchoolId = user.SchoolId,
            Token = token,
            ExpiresAtUtc = expiresAtUtc
        };

        return Ok(response);
    }
}
