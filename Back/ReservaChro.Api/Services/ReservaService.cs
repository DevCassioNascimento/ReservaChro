using Microsoft.EntityFrameworkCore;
using ReservaChro.Application.DTOs.Reserva;
using ReservaChro.Domain.Entities;
using ReservaChro.Domain.Enums;
using ReservaChro.Infrastructure.Data;

namespace ReservaChro.Api.Services;

public interface IReservaService
{
    Task<List<ReservaResponseDto>> GetPendentesBySchoolAsync(Guid schoolId);
    Task<ReservaResponseDto?> GetByIdAsync(Guid id);
    Task<bool> ConfirmarReservaAsync(Guid id, Guid schoolId);
    Task<bool> RecusarReservaAsync(Guid id, Guid schoolId);
    Task<int> GetQuantidadeDisponivelAsync(Guid schoolId, DateTime data);
}

public sealed class ReservaService : IReservaService
{
    private readonly AppDbContext _dbContext;

    public ReservaService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private DbSet<Reserva> Reservas => _dbContext.Set<Reserva>();

    public async Task<List<ReservaResponseDto>> GetPendentesBySchoolAsync(Guid schoolId)
    {
        if (schoolId == Guid.Empty) return new List<ReservaResponseDto>();

        var reservas = await Reservas
            .AsNoTracking()
            .Where(r => r.SchoolId == schoolId && r.Status == StatusReserva.Pendente)
            .OrderBy(r => r.DataReserva)
            .ThenBy(r => r.HorarioInicio)
            .ToListAsync();

        var professorIds = reservas.Select(r => r.ProfessorId).Distinct().ToList();
        var professores = await _dbContext.Users
            .AsNoTracking()
            .Where(u => professorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name);

        return reservas.Select(r => new ReservaResponseDto
        {
            Id = r.Id,
            ProfessorId = r.ProfessorId,
            ProfessorNome = professores.TryGetValue(r.ProfessorId, out var nome) ? nome : "Professor",
            SchoolId = r.SchoolId,
            DataReserva = r.DataReserva,
            HorarioInicio = r.HorarioInicio,
            HorarioFim = r.HorarioFim,
            Quantidade = r.Quantidade,
            Status = r.Status,
            DataCriacao = r.DataCriacao
        }).ToList();
    }

    public async Task<ReservaResponseDto?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty) return null;

        var reserva = await Reservas
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reserva is null) return null;

        var professor = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == reserva.ProfessorId);

        return new ReservaResponseDto
        {
            Id = reserva.Id,
            ProfessorId = reserva.ProfessorId,
            ProfessorNome = professor?.Name ?? "Professor",
            SchoolId = reserva.SchoolId,
            DataReserva = reserva.DataReserva,
            HorarioInicio = reserva.HorarioInicio,
            HorarioFim = reserva.HorarioFim,
            Quantidade = reserva.Quantidade,
            Status = reserva.Status,
            DataCriacao = reserva.DataCriacao
        };
    }

    public async Task<bool> ConfirmarReservaAsync(Guid id, Guid schoolId)
    {
        if (id == Guid.Empty || schoolId == Guid.Empty)
            return false;

        var reserva = await Reservas.FirstOrDefaultAsync(r => r.Id == id);
        if (reserva is null || reserva.SchoolId != schoolId)
            return false;

        if (reserva.Status != StatusReserva.Pendente)
            throw new InvalidOperationException("Apenas reservas pendentes podem ser confirmadas.");

        // Verificar disponibilidade
        var disponivel = await GetQuantidadeDisponivelAsync(schoolId, reserva.DataReserva);
        if (disponivel < reserva.Quantidade)
            throw new InvalidOperationException($"Não há máquinas suficientes disponíveis. Disponível: {disponivel}, Solicitado: {reserva.Quantidade}");

        reserva.Confirmar();

        try
        {
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> RecusarReservaAsync(Guid id, Guid schoolId)
    {
        if (id == Guid.Empty || schoolId == Guid.Empty)
            return false;

        var reserva = await Reservas.FirstOrDefaultAsync(r => r.Id == id);
        if (reserva is null || reserva.SchoolId != schoolId)
            return false;

        if (reserva.Status != StatusReserva.Pendente)
            throw new InvalidOperationException("Apenas reservas pendentes podem ser recusadas.");

        reserva.Recusar();

        try
        {
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<int> GetQuantidadeDisponivelAsync(Guid schoolId, DateTime data)
    {
        if (schoolId == Guid.Empty) return 0;

        // Buscar estoque total da escola
        var school = await _dbContext.Schools
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == schoolId);

        if (school is null) return 0;

        var estoqueTotal = school.QuantidadeEstoque;

        // Calcular quantidade já reservada/confirmada para a data
        var quantidadeReservada = await Reservas
            .AsNoTracking()
            .Where(r => r.SchoolId == schoolId
                && r.DataReserva.Date == data.Date
                && (r.Status == StatusReserva.Confirmada || r.Status == StatusReserva.EmUso))
            .SumAsync(r => r.Quantidade);

        var disponivel = estoqueTotal - quantidadeReservada;
        return disponivel < 0 ? 0 : disponivel;
    }
}
