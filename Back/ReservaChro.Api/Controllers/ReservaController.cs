using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaChro.Api.Services;
using ReservaChro.Domain.Enums;

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
            return Ok(reservas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao listar reservas pendentes.", detail = ex.Message });
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
}
