using System.Threading.Tasks;

namespace ArtiX.Domain.Auth;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string userNameOrEmail, string password);
}
