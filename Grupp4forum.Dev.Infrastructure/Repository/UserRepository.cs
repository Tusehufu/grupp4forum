using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Grupp4forum.Dev.Infrastructure.Configuration;
using Grupp4forum.Dev.Infrastructure.Models;
using Isopoh.Cryptography.Argon2;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Grupp4forum.Dev.Infrastructure.Repository
{
    public class UserRepository
    {
        private readonly DatabaseSettings _databaseSettings;

        public UserRepository(IOptions<DatabaseSettings> databaseSettings)
        {
            _databaseSettings = databaseSettings.Value;
        }

        // Hämta alla användare och deras roller
        public async Task<IEnumerable<User>> FindAllWithRoles()
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = @"
        SELECT u.Id AS Id, u.username, u.email, u.password_hash AS PasswordHash, 
               u.created_at AS CreatedAt, u.updated_at AS UpdatedAt, u.role_id AS RoleId,
               r.role_id AS RoleIdAlias, r.Name
        FROM Users u
        JOIN Roles r ON u.role_id = r.role_id";  // Matchar mot role_id

            var userRoleMapping = await connection.QueryAsync<User, Role, User>(
                query,
                (user, role) =>
                {
                    user.Role = role;  // Mappa användarens roll
                    return user;
                },
                splitOn: "RoleIdAlias"  // Ange RoleId som split-kolumnen
            );

            return userRoleMapping;
        }



        // Hämta en användare efter ID med tillhörande roll
        public async Task<User> FindByIdWithRole(int id)
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = @"
        SELECT u.Id AS Id, u.Username, u.Email, u.password_hash AS PasswordHash, 
               u.created_at AS CreatedAt, u.updated_at AS UpdatedAt, u.role_id AS RoleId,
               r.role_id AS RoleIdAlias, r.Name AS RoleName
        FROM Users u
        JOIN Roles r ON u.role_id = r.role_id
        WHERE u.Id = @Id";

            var userRoleMapping = await connection.QueryAsync<User, Role, User>(
                query,
                (user, role) =>
                {
                    user.Role = role;
                    return user;
                },
                new { Id = id },
                splitOn: "RoleIdAlias"  // Se till att splitOn använder RoleId korrekt
            );

            return userRoleMapping.FirstOrDefault();
        }



        // Lägg till en användare
        public async Task<int> Add(User user)
        {
            // Hasha användarens lösenord, username och email
            string hashedPassword = Argon2.Hash(user.Password);
            string hashedUsername = Argon2.Hash(user.Username);
            string hashedEmail = Argon2.Hash(user.Email);

            // Spara de hashade värdena
            user.PasswordHash = hashedPassword;
            user.Username = hashedUsername;
            user.Email = hashedEmail;

            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = @"
        INSERT INTO Users (Username, Email, password_hash, role_id, created_at, updated_at)
        OUTPUT INSERTED.Id
        VALUES (@Username, @Email, @PasswordHash, @RoleId, @CreatedAt, @UpdatedAt)";
            return await connection.ExecuteScalarAsync<int>(query, user);
        }


        // Uppdatera en användare
        public async Task<bool> Update(User user)
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = @"
                UPDATE Users
                SET Username = @Username, Email = @Email,
                    PasswordHash = @PasswordHash, role_id = @RoleId, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            int affectedRows = await connection.ExecuteAsync(query, user);
            return affectedRows > 0;
        }

        // Ta bort en användare med soft delete (anonymisering och inaktivering)
        public async Task<bool> Delete(int id)
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = @"
        UPDATE Users
        SET is_active = 0,                                 -- Markera användaren som inaktiv
            username = CONCAT('deleted_user_', Id),        -- Anonymisera användarnamnet
            email = CONCAT('deleted_', Id, '@example.com'),-- Anonymisera e-postadressen
            password_hash = CONCAT('$argon2id$v=19$m=65536,t=2,p=1$', CONVERT(varchar(36), NEWID())) -- Sätt ett ogiltigt hash
        WHERE Id = @Id"; 

    int affectedRows = await connection.ExecuteAsync(query, new { Id = id });
            return affectedRows > 0;
        }



        // Hitta användare baserat på användarnamn
        public async Task<User> FindByUsername(string username)
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = @"
                 SELECT u.*, r.role_id AS RoleIdAlias, r.Name
        FROM Users u
        JOIN Roles r ON u.role_id = r.role_id
        WHERE u.Username = @Username";

            var userRoleMapping = await connection.QueryAsync<User, Role, User>(
                query,
                (user, role) =>
                {
                    user.Role = role;
                    return user;
                },
                new { Username = username },
                 splitOn: "RoleIdAlias"
            );

            return userRoleMapping.FirstOrDefault();
        }
    }
}
