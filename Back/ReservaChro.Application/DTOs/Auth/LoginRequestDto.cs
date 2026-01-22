using System.Text.Json.Serialization;

namespace ReservaChro.Application.DTOs.Auth;

public sealed class LoginRequestDto
{
    // Compatível com backend antigo e chamadas que mandam "username"
    [JsonPropertyName("username")]
    public string Username { get; init; } = string.Empty;

    // Compatível com chamadas que mandam "email"
    // (não quebra, só adiciona suporte)
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; init; } = string.Empty;

    // Fonte final para login: se vier email, usa email; senão username.
    public string GetLogin() =>
        !string.IsNullOrWhiteSpace(Email) ? Email :
        !string.IsNullOrWhiteSpace(Username) ? Username :
        string.Empty;
}
