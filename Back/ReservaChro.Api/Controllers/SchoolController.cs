using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaChro.Api.Services;
using ReservaChro.Domain.Enums;

namespace ReservaChro.Api.Controllers;

[ApiController]
[Route("school")]
[Authorize]
public sealed class SchoolController : ControllerBase
{
    private readonly ISchoolService _service;

    public SchoolController(ISchoolService service)
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
    /// Obtém a quantidade de estoque da escola do usuário
    /// </summary>
    [HttpGet("estoque")]
    public async Task<IActionResult> GetEstoque()
    {
        if (!TryGetSchoolId(out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        try
        {
            var quantidade = await _service.GetQuantidadeEstoqueAsync(schoolId);
            return Ok(new { quantidade, schoolId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao obter estoque.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza a quantidade de estoque da escola (apenas para TI)
    /// </summary>
    [HttpPut("estoque")]
    [Authorize(Roles = nameof(Role.TI))]
    public async Task<IActionResult> UpdateEstoque([FromBody] UpdateEstoqueRequest request)
    {
        if (!TryGetSchoolId(out var schoolId))
            return Unauthorized(new { message = "SchoolId não encontrado no token." });

        if (request is null || request.Quantidade < 0)
            return BadRequest(new { message = "Quantidade deve ser um número maior ou igual a 0." });

        try
        {
            var sucesso = await _service.UpdateQuantidadeEstoqueAsync(schoolId, request.Quantidade);
            return Ok(new { message = "Estoque atualizado com sucesso.", quantidade = request.Quantidade, schoolId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("não encontrada"))
                return NotFound(new { message = ex.Message });
            return StatusCode(500, new { message = "Erro ao atualizar estoque.", detail = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao atualizar estoque.", detail = ex.Message });
        }
    }
}

public sealed class UpdateEstoqueRequest
{
    public int Quantidade { get; init; }
}
