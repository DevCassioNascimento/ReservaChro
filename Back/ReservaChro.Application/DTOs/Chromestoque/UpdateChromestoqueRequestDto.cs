namespace ReservaChro.Application.DTOs.Chromestoque;

public sealed class UpdateChromestoqueRequestDto
{
    public string? NomeMaquina { get; init; }
    public string? Modelo { get; init; }
    public bool? Ativo { get; init; }
}
