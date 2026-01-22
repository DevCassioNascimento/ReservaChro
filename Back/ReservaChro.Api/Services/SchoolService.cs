using Microsoft.EntityFrameworkCore;
using ReservaChro.Infrastructure.Data;

namespace ReservaChro.Api.Services;

public interface ISchoolService
{
    Task<int> GetQuantidadeEstoqueAsync(Guid schoolId);
    Task<bool> UpdateQuantidadeEstoqueAsync(Guid schoolId, int quantidade);
}

public sealed class SchoolService : ISchoolService
{
    private readonly AppDbContext _dbContext;

    public SchoolService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> GetQuantidadeEstoqueAsync(Guid schoolId)
    {
        if (schoolId == Guid.Empty) return 0;

        var school = await _dbContext.Schools
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == schoolId);

        return school?.QuantidadeEstoque ?? 0;
    }

    public async Task<bool> UpdateQuantidadeEstoqueAsync(Guid schoolId, int quantidade)
    {
        if (schoolId == Guid.Empty)
            throw new ArgumentException("SchoolId é obrigatório.", nameof(schoolId));

        if (quantidade < 0)
            throw new ArgumentException("A quantidade não pode ser negativa.", nameof(quantidade));

        var school = await _dbContext.Schools.FirstOrDefaultAsync(s => s.Id == schoolId);
        if (school is null)
            throw new InvalidOperationException($"Escola com ID {schoolId} não encontrada.");

        school.AtualizarQuantidadeEstoque(quantidade);

        try
        {
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Falha ao atualizar estoque no banco de dados.", ex);
        }
    }
}
