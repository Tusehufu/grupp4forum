using Isopoh.Cryptography.Argon2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Isopoh.Cryptography.Argon2;

namespace Grupp4forum.Dev.Infrastructure.Services
{
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
}
