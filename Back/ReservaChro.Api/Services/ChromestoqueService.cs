using Microsoft.EntityFrameworkCore;
using ReservaChro.Application.DTOs.Chromestoque;
using ReservaChro.Domain.Entities;
using ReservaChro.Infrastructure.Data;

namespace ReservaChro.Api.Services;

public interface IChromestoqueService
{
    Task<ChromestoqueResponseDto> CreateAsync(CreateChromestoqueRequestDto request, Guid schoolId);
    Task<ChromestoqueResponseDto?> GetByIdAsync(Guid id);
    Task<List<ChromestoqueResponseDto>> GetBySchoolAsync(Guid schoolId);
    Task<ChromestoqueResponseDto?> UpdateAsync(Guid id, UpdateChromestoqueRequestDto request);
    Task<bool> DeleteAsync(Guid id);
    Task<int> GetCountBySchoolAsync(Guid schoolId);
}

public sealed class ChromestoqueService : IChromestoqueService
{
    private readonly AppDbContext _dbContext;

    public ChromestoqueService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // ✅ IMPORTANTE:
    // Para reduzir chance de erro por nome de DbSet ("Chromestoque" vs "Chromestoques"),
    // usamos Set<Chromestoque>() em vez de depender de uma property específica.
    private DbSet<Chromestoque> Chromes => _dbContext.Set<Chromestoque>();

    public async Task<ChromestoqueResponseDto> CreateAsync(CreateChromestoqueRequestDto request, Guid schoolId)
    {
        if (schoolId == Guid.Empty)
            throw new ArgumentException("SchoolId é obrigatório.", nameof(schoolId));

        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var numeroSerie = (request.NumeroSerie ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(numeroSerie))
            throw new InvalidOperationException("Número de série é obrigatório.");

        // Validar se a escola existe
        var schoolExists = await _dbContext.Schools.AnyAsync(s => s.Id == schoolId);
        if (!schoolExists)
            throw new InvalidOperationException($"Escola com ID {schoolId} não encontrada.");

        // Validar se o número de série já existe (normalizado)
        var serieExists = await Chromes
            .AsNoTracking()
            .AnyAsync(c => c.NumeroSerie == numeroSerie);

        if (serieExists)
            throw new InvalidOperationException($"Chromebook com número de série '{numeroSerie}' já existe.");

        var chromestoque = new Chromestoque(
            request.NomeMaquina,
            numeroSerie,
            request.Modelo,
            request.DataAquisicao,
            schoolId
        );

        try
        {
            await Chromes.AddAsync(chromestoque);
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Ajuda a debugar sem estourar 500 “cego”
            throw new InvalidOperationException("Falha ao salvar no banco. Verifique migrations/tabela/constraints.", ex);
        }

        return MapToDto(chromestoque);
    }

    public async Task<ChromestoqueResponseDto?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty) return null;

        var chromestoque = await Chromes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        return chromestoque is null ? null : MapToDto(chromestoque);
    }

    public async Task<List<ChromestoqueResponseDto>> GetBySchoolAsync(Guid schoolId)
    {
        if (schoolId == Guid.Empty) return new List<ChromestoqueResponseDto>();

        var chromestoque = await Chromes
            .AsNoTracking()
            .Where(c => c.SchoolId == schoolId)
            .OrderByDescending(c => c.DataAquisicao)
            .ToListAsync();

        return chromestoque.Select(MapToDto).ToList();
    }

    public async Task<ChromestoqueResponseDto?> UpdateAsync(Guid id, UpdateChromestoqueRequestDto request)
    {
        if (id == Guid.Empty) return null;
        if (request is null) throw new ArgumentNullException(nameof(request));

        var chromestoque = await Chromes.FirstOrDefaultAsync(c => c.Id == id);
        if (chromestoque is null)
            return null;

        if (!string.IsNullOrWhiteSpace(request.NomeMaquina))
            chromestoque.AtualizarNomeMaquina(request.NomeMaquina.Trim());

        if (!string.IsNullOrWhiteSpace(request.Modelo))
            chromestoque.AtualizarModelo(request.Modelo.Trim());

        if (request.Ativo.HasValue)
        {
            if (request.Ativo.Value) chromestoque.Ativar();
            else chromestoque.Desativar();
        }

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Falha ao atualizar no banco. Verifique constraints e migrations.", ex);
        }

        return MapToDto(chromestoque);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (id == Guid.Empty) return false;

        var chromestoque = await Chromes.FirstOrDefaultAsync(c => c.Id == id);
        if (chromestoque is null)
            return false;

        Chromes.Remove(chromestoque);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Falha ao remover no banco. Verifique relacionamentos/constraints.", ex);
        }

        return true;
    }

    public async Task<int> GetCountBySchoolAsync(Guid schoolId)
    {
        if (schoolId == Guid.Empty) return 0;

        return await Chromes
            .AsNoTracking()
            .CountAsync(c => c.SchoolId == schoolId && c.Ativo);
    }

    private static ChromestoqueResponseDto MapToDto(Chromestoque chromestoque)
    {
        return new ChromestoqueResponseDto
        {
            Id = chromestoque.Id,
            NomeMaquina = chromestoque.NomeMaquina,
            NumeroSerie = chromestoque.NumeroSerie,
            Modelo = chromestoque.Modelo,
            Ativo = chromestoque.Ativo,
            DataAquisicao = chromestoque.DataAquisicao,
            SchoolId = chromestoque.SchoolId
        };
    }
}
