using Isopoh.Cryptography.Argon2;

public class PasswordService
{
    // Hash(a) lösenord med Argon2
    public string HashPassword(string password)
    {
        return Argon2.Hash(password);
    }

    // Verifiera att ett lösenord matchar en given hash
    public bool VerifyPassword(string password, string hashedPassword)
    {
        return Argon2.Verify(hashedPassword, password);
    }
}
