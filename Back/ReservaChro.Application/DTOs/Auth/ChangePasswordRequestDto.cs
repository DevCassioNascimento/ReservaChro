using System.Text.Json.Serialization;

namespace ReservaChro.Application.DTOs.Auth;

public sealed class ChangePasswordRequestDto
{
    [JsonPropertyName("currentPassword")]
    public string CurrentPassword { get; init; } = string.Empty;

    [JsonPropertyName("newPassword")]
    public string NewPassword { get; init; } = string.Empty;
}
