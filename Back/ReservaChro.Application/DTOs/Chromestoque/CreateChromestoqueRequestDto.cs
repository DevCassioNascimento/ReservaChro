namespace ReservaChro.Application.DTOs.Chromestoque;

public sealed class CreateChromestoqueRequestDto
{
    public string NomeMaquina { get; init; } = string.Empty;
    public string NumeroSerie { get; init; } = string.Empty;
    public string Modelo { get; init; } = string.Empty;
    public DateTime DataAquisicao { get; init; }
}
