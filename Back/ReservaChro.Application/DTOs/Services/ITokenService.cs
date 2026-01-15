using ReservaChro.Domain.Entities;

namespace ReservaChro.Application.Auth;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) Generate(User user);
}
