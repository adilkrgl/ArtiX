namespace ArtiX.Domain.Auth;

public interface IPasswordHasher
{
    void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);

    bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt);
}
