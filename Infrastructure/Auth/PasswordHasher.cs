using Application.Interfaces;

namespace Infrastructure.Auth;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string passwordHash, string password) => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
