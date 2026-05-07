using Application.Interfaces;

namespace Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string value)
    {
        return BCrypt.Net.BCrypt.HashPassword(value);
    }

    public bool Verify(string value, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(value, hash);
    }
}
