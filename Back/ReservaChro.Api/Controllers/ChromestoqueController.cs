using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaChro.Api.Services;
using ReservaChro.Application.DTOs.Chromestoque;
using ReservaChro.Domain.Enums;

namespace ReservaChro.Api.Controllers;

[ApiController]
[Route("chromestoque")]
[Authorize]
public sealed class ChromestoqueController : ControllerBase
{
    private readonly IChromestoqueService _service;

    public ChromestoqueController(IChromestoqueService service)
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
    /// Adiciona um novo chromebook ao estoque (apenas para TI)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = nameof(Role.TI))]
    public async Task<IActionResult> Create([FromBody] CreateChromestoqueRequestDto request)
    {
        if (!TryGetSchoolId(out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        try
        {
            var result = await _service.CreateAsync(request, schoolId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao criar chromebook.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Obtém um chromebook específico pelo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (!TryGetSchoolId(out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        try
        {
            var result = await _service.GetByIdAsync(id);
            if (result is null)
                return NotFound();

            // Isolamento por escola
            if (result.SchoolId != schoolId)
                return Forbid();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar chromebook.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Lista todos os chromebooks da escola do usuário
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBySchool()
    {
        if (!TryGetSchoolId(out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        try
        {
            var result = await _service.GetBySchoolAsync(schoolId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao listar chromebooks.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza os dados de um chromebook (apenas para TI)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(Role.TI))]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChromestoqueRequestDto request)
    {
        if (!TryGetSchoolId(out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        try
        {
            var chromestoque = await _service.GetByIdAsync(id);
            if (chromestoque is null)
                return NotFound();

            if (chromestoque.SchoolId != schoolId)
                return Forbid();

            var result = await _service.UpdateAsync(id, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao atualizar chromebook.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Remove um chromebook do estoque (apenas para TI)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(Role.TI))]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryGetSchoolId(out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        try
        {
            var chromestoque = await _service.GetByIdAsync(id);
            if (chromestoque is null)
                return NotFound();

            if (chromestoque.SchoolId != schoolId)
                return Forbid();

            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao remover chromebook.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Obtém a contagem de chromebooks ativos na escola
    /// </summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
    {
        if (!TryGetSchoolId(out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        try
        {
            var count = await _service.GetCountBySchoolAsync(schoolId);
            return Ok(new { count, schoolId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao obter contagem.", detail = ex.Message });
        }
    }
}
