using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaChro.Api.Services;
using ReservaChro.Application.DTOs.Reserva;
using ReservaChro.Domain.Enums;
using System.Security.Claims;

namespace ReservaChro.Api.Controllers;

[ApiController]
[Route("reservas")]
[Authorize]
public sealed class ReservaController : ControllerBase
{
    private readonly IReservaService _service;

    public ReservaController(IReservaService service)
    {
        _service = service;
    }

    private bool TryGetSchoolId(out Guid schoolId)
    {
        schoolId = Guid.Empty;

        var claim =
            User.FindFirst("schoolId") ??
            User.FindFirst("SchoolId") ??
            User.FindFirst("schoolID") ??
            User.FindFirst("school_id");

        return claim is not null && Guid.TryParse(claim.Value, out schoolId);
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId") ?? User.FindFirst("UserId");
        return claim is not null && Guid.TryParse(claim.Value, out userId);
    }

    /// <summary>
    /// Cria uma nova reserva (apenas para Professores)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = nameof(Role.Professor))]
    public async Task<IActionResult> Create([FromBody] CreateReservaRequestDto request)
    {
        if (!TryGetSchoolId(out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        if (!TryGetUserId(out var professorId))
            return Unauthorized(new { message = "UserId não encontrado no token." });

        if (request is null)
            return BadRequest(new { message = "Dados da reserva são obrigatórios." });

        try
        {
            var reserva = await _service.CreateAsync(request, professorId, schoolId);
            return CreatedAtAction(nameof(GetById), new { id = reserva.Id }, reserva);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao criar reserva.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Obtém uma reserva específica pelo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (!TryGetSchoolId(out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        try
        {
            var reserva = await _service.GetByIdAsync(id);
            if (reserva is null)
                return NotFound();

            // Verificar se a reserva pertence à escola do usuário
            if (reserva.SchoolId != schoolId)
                return Forbid();

            return Ok(reserva);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar reserva.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Lista reservas pendentes da escola (apenas para TI)
    /// </summary>
    [HttpGet("pendentes")]
    [Authorize(Roles = nameof(Role.TI))]
    public async Task<IActionResult> GetPendentes()
    {
        if (!TryGetSchoolId(out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        try
        {
            var reservas = await _service.GetPendentesBySchoolAsync(schoolId);
            // Log para debug
            System.Diagnostics.Debug.WriteLine($"[API] GET /reservas/pendentes - SchoolId: {schoolId}, Total: {reservas.Count}");
            if (reservas.Count > 0)
            {
                var primeira = reservas[0];
                System.Diagnostics.Debug.WriteLine($"[API] Primeira reserva - Id: {primeira.Id}, ProfessorNome: {primeira.ProfessorNome}, DataReserva: {primeira.DataReserva}, Quantidade: {primeira.Quantidade}, Status: {primeira.Status}");
            }
            // Retornar como array direto para compatibilidade com frontend
            return Ok(reservas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao listar reservas pendentes.", detail = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Confirma uma reserva (apenas para TI)
    /// </summary>
    [HttpPut("{id:guid}/confirmar")]
    [Authorize(Roles = nameof(Role.TI))]
    public async Task<IActionResult> Confirmar(Guid id)
    {
        if (!TryGetSchoolId(out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        try
        {
            var sucesso = await _service.ConfirmarReservaAsync(id, schoolId);
            if (!sucesso)
                return NotFound(new { message = "Reserva não encontrada ou não pertence à sua escola." });

            var reserva = await _service.GetByIdAsync(id);
            return Ok(new { message = "Reserva confirmada com sucesso.", reserva });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao confirmar reserva.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Recusa uma reserva (apenas para TI)
    /// </summary>
    [HttpPut("{id:guid}/recusar")]
    [Authorize(Roles = nameof(Role.TI))]
    public async Task<IActionResult> Recusar(Guid id)
    {
        if (!TryGetSchoolId(out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        try
        {
            var sucesso = await _service.RecusarReservaAsync(id, schoolId);
            if (!sucesso)
                return NotFound(new { message = "Reserva não encontrada ou não pertence à sua escola." });

            return Ok(new { message = "Reserva recusada com sucesso." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao recusar reserva.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Obtém quantidade disponível para uma data específica
    /// </summary>
    [HttpGet("disponivel")]
    public async Task<IActionResult> GetDisponivel([FromQuery] DateTime data)
    {
        if (!TryGetSchoolId(out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        try
        {
            var disponivel = await _service.GetQuantidadeDisponivelAsync(schoolId, data);
            return Ok(new { data = data.Date, disponivel, schoolId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao obter disponibilidade.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Lista TODAS as reservas da escola (debug - apenas para TI)
    /// </summary>
    [HttpGet("todas")]
    [Authorize(Roles = nameof(Role.TI))]
    public async Task<IActionResult> GetTodas()
    {
        if (!TryGetSchoolId(out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        try
        {
            var reservas = await _service.GetTodasBySchoolAsync(schoolId);
            return Ok(new { schoolId, reservas, total = reservas.Count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao listar reservas.", detail = ex.Message });
        }
    }
}
