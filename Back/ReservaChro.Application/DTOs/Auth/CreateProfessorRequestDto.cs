using System.Text.Json.Serialization;

namespace ReservaChro.Application.DTOs.Auth;

public sealed class CreateProfessorRequestDto
{
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}
