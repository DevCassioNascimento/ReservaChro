using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaChro.Api.Services;
using ReservaChro.Application.DTOs.Auth;
using ReservaChro.Domain.Entities;
using ReservaChro.Domain.Enums;
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

    /// <summary>
    /// Cria um novo professor (apenas para TI)
    /// </summary>
    [HttpPost("professor")]
    [Authorize(Roles = nameof(Role.TI))]
    public async Task<IActionResult> CreateProfessor([FromBody] CreateProfessorRequestDto? request)
    {
        if (request is null)
            return BadRequest(new { message = "Dados são obrigatórios." });

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "E-mail é obrigatório." });

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Nome é obrigatório." });

        // Obter SchoolId do token do TI
        var schoolIdClaim = User.FindFirst("schoolId") ??
                           User.FindFirst("SchoolId") ??
                           User.FindFirst("schoolID") ??
                           User.FindFirst("school_id");

        if (schoolIdClaim is null || !Guid.TryParse(schoolIdClaim.Value, out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        // Verificar se o e-mail já existe
        var emailNormalized = request.Email.Trim().ToLowerInvariant();
        var emailExists = await _dbContext.Users
            .AnyAsync(u => u.Email.ToLower() == emailNormalized);

        if (emailExists)
            return Conflict(new { message = "E-mail já cadastrado." });

        // Senha padrão
        var senhaPadrao = "123456"; // Senha padrão que o professor pode alterar depois

        try
        {
            // Criar novo professor
            var professor = new User(
                name: request.Name.Trim(),
                email: emailNormalized,
                role: Role.Professor,
                schoolId: schoolId);

            // Definir senha padrão (TEMPORÁRIO: sem hash)
            professor.SetPassword(senhaPadrao, "TEMP");

            _dbContext.Users.Add(professor);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Login),
                new { },
                new
                {
                    message = "Professor criado com sucesso.",
                    id = professor.Id,
                    email = professor.Email,
                    name = professor.Name,
                    senhaPadrao = senhaPadrao // Informar a senha padrão
                });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao criar professor.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Lista professores da escola (apenas para TI)
    /// </summary>
    [HttpGet("professores")]
    [Authorize(Roles = nameof(Role.TI))]
    public async Task<IActionResult> GetProfessores()
    {
        // Obter SchoolId do token do TI
        var schoolIdClaim = User.FindFirst("schoolId") ??
                           User.FindFirst("SchoolId") ??
                           User.FindFirst("schoolID") ??
                           User.FindFirst("school_id");

        if (schoolIdClaim is null || !Guid.TryParse(schoolIdClaim.Value, out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        try
        {
            var professores = await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.SchoolId == schoolId && u.Role == Role.Professor)
                .OrderBy(u => u.Name)
                .Select(u => new ProfessorResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email
                })
                .ToListAsync();

            return Ok(professores);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao listar professores.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Exclui um professor (apenas para TI)
    /// </summary>
    [HttpDelete("professor/{id:guid}")]
    [Authorize(Roles = nameof(Role.TI))]
    public async Task<IActionResult> DeleteProfessor(Guid id)
    {
        // Obter SchoolId do token do TI
        var schoolIdClaim = User.FindFirst("schoolId") ??
                           User.FindFirst("SchoolId") ??
                           User.FindFirst("schoolID") ??
                           User.FindFirst("school_id");

        if (schoolIdClaim is null || !Guid.TryParse(schoolIdClaim.Value, out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        try
        {
            var professor = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.SchoolId == schoolId && u.Role == Role.Professor);

            if (professor is null)
                return NotFound(new { message = "Professor não encontrado ou não pertence à sua escola." });

            // Verificar se o professor tem reservas ativas (pendentes, confirmadas ou em uso)
            var temReservasAtivas = await _dbContext.Set<Reserva>()
                .AnyAsync(r => r.ProfessorId == id && 
                              (r.Status == StatusReserva.Pendente || 
                               r.Status == StatusReserva.Confirmada || 
                               r.Status == StatusReserva.EmUso));

            if (temReservasAtivas)
                return BadRequest(new { message = "Não é possível excluir professor com reservas ativas." });

            _dbContext.Users.Remove(professor);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Professor excluído com sucesso." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao excluir professor.", detail = ex.Message });
        }
    }
}
