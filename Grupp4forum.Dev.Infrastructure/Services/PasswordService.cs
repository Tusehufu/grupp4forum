using Isopoh.Cryptography.Argon2;

public class PasswordService
{
    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (hashedPassword == null)
        {
            Console.WriteLine("Hashed password is null!");
            return false;
        }

        Console.WriteLine($"Verifying password: {password}");
        Console.WriteLine($"Against hash: {hashedPassword}");

        try
        {
            return Argon2.Verify(hashedPassword, password);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during password verification: {ex.Message}");
            return false;
        }
    }

}
