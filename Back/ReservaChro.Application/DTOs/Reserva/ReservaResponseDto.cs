using ReservaChro.Domain.Enums;

namespace ReservaChro.Application.DTOs.Reserva;

public sealed class ReservaResponseDto
{
    public Guid Id { get; init; }
    public Guid ProfessorId { get; init; }
    public string ProfessorNome { get; init; } = string.Empty;
    public Guid SchoolId { get; init; }
    public DateTime DataReserva { get; init; }
    public TimeSpan HorarioInicio { get; init; }
    public TimeSpan HorarioFim { get; init; }
    public int Quantidade { get; init; }
    public int Status { get; init; } // Serializado como int (1=Pendente, 2=Confirmada, etc)
    public DateTime DataCriacao { get; init; }
}
