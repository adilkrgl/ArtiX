using System.Threading.Tasks;
using ArtiX.Domain.Auth;

namespace ArtiX.Application.Auth;

public interface IAuthService : Domain.Auth.IAuthService
{
    Task<AuthResult> LoginAsync(string userNameOrEmail, string password);
}
