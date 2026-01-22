namespace ReservaChro.Application.DTOs.Reserva;

public sealed class CreateReservaRequestDto
{
    public DateTime DataReserva { get; init; }
    public TimeSpan HorarioInicio { get; init; }
    public TimeSpan HorarioFim { get; init; }
    public int Quantidade { get; init; }
}
