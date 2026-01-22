using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaChro.Api.Services;
using ReservaChro.Application.DTOs.Auth;
using ReservaChro.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

    /// <summary>
    /// Altera a senha do usuário logado
    /// </summary>
    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto? request)
    {
        if (request is null)
            return BadRequest(new { message = "Dados são obrigatórios." });

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            return BadRequest(new { message = "Senha atual é obrigatória." });

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest(new { message = "Nova senha é obrigatória." });

        if (request.NewPassword.Length < 4)
            return BadRequest(new { message = "A nova senha deve ter pelo menos 4 caracteres." });

        // Obter ID do usuário do token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? 
                         User.FindFirst("userId") ?? 
                         User.FindFirst("UserId") ??
                         User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(new { message = "Token inválido ou expirado." });

        // Buscar usuário
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return NotFound(new { message = "Usuário não encontrado." });

        // Validar senha atual (TEMPORÁRIO: comparação direta)
        if (user.PasswordHash != request.CurrentPassword)
            return Unauthorized(new { message = "Senha atual incorreta." });

        // Atualizar senha (TEMPORÁRIO: sem hash, apenas atualiza PasswordHash)
        // Quando implementar hash real, usar: BCrypt ou similar
        user.SetPassword(request.NewPassword, user.PasswordSalt);

        try
        {
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Senha alterada com sucesso." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao alterar senha.", detail = ex.Message });
        }
    }
}
