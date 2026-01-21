namespace ReservaChro.Application.DTOs.Auth;

public sealed class LoginResponseDto
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;

    // ✅ agora é Email (em vez de Username)
    public string Email { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;
    public Guid? SchoolId { get; init; }

    // 🔐 JWT
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}
