using ReservaChro.Domain.Enums;

namespace ReservaChro.Domain.Entities;

public sealed class Reserva
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid ProfessorId { get; private set; }

    public Guid SchoolId { get; private set; }

    public DateTime DataReserva { get; private set; }

    public TimeSpan HorarioInicio { get; private set; }

    public TimeSpan HorarioFim { get; private set; }

    public int Quantidade { get; private set; }

    public StatusReserva Status { get; private set; } = StatusReserva.Pendente;

    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;

    // EF Core
    private Reserva() { }

    public Reserva(
        Guid professorId,
        Guid schoolId,
        DateTime dataReserva,
        TimeSpan horarioInicio,
        TimeSpan horarioFim,
        int quantidade)
    {
        if (professorId == Guid.Empty)
            throw new ArgumentException("ProfessorId é obrigatório.", nameof(professorId));
        if (schoolId == Guid.Empty)
            throw new ArgumentException("SchoolId é obrigatório.", nameof(schoolId));
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));
        if (horarioFim <= horarioInicio)
            throw new ArgumentException("Horário fim deve ser maior que horário início.", nameof(horarioFim));

        ProfessorId = professorId;
        SchoolId = schoolId;
        // Garantir que a data está em UTC para PostgreSQL
        DataReserva = dataReserva.Kind == DateTimeKind.Utc 
            ? dataReserva.Date 
            : DateTime.SpecifyKind(dataReserva.Date, DateTimeKind.Utc);
        HorarioInicio = horarioInicio;
        HorarioFim = horarioFim;
        Quantidade = quantidade;
    }

    public void Confirmar()
    {
        if (Status != StatusReserva.Pendente)
            throw new InvalidOperationException("Apenas reservas pendentes podem ser confirmadas.");

        Status = StatusReserva.Confirmada;
    }

    public void Recusar()
    {
        if (Status != StatusReserva.Pendente)
            throw new InvalidOperationException("Apenas reservas pendentes podem ser recusadas.");

        Status = StatusReserva.Recusada;
    }

    public void IniciarUso()
    {
        if (Status != StatusReserva.Confirmada)
            throw new InvalidOperationException("Apenas reservas confirmadas podem iniciar uso.");

        Status = StatusReserva.EmUso;
    }

    public void Concluir()
    {
        if (Status != StatusReserva.EmUso)
            throw new InvalidOperationException("Apenas reservas em uso podem ser concluídas.");

        Status = StatusReserva.Concluida;
    }
}
