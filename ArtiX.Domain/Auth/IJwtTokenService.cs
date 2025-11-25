using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Auth;

public interface IJwtTokenService
{
    string GenerateToken(User user, IEnumerable<string> roles);
}
