namespace ReservaChro.Application.DTOs.Chromestoque;

public sealed class ChromestoqueResponseDto
{
    public Guid Id { get; init; }
    public string NomeMaquina { get; init; } = string.Empty;
    public string NumeroSerie { get; init; } = string.Empty;
    public string Modelo { get; init; } = string.Empty;
    public bool Ativo { get; init; }
    public DateTime DataAquisicao { get; init; }
    public Guid SchoolId { get; init; }
}
