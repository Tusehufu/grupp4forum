using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Grupp4forum.Dev.Infrastructure.Configuration;
using Grupp4forum.Dev.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Grupp4forum.Dev.Infrastructure.Repository
{
    public class RoleRepository
    {
        private readonly DatabaseSettings _databaseSettings;

        public RoleRepository(IOptions<DatabaseSettings> databaseSettings)
        {
            _databaseSettings = databaseSettings.Value;
        }

        // Hämta alla roller
        public async Task<IEnumerable<Role>> GetAllRoles()
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = @"
        SELECT r.role_id AS RoleId, r.Name, 
               u.Id AS Id, u.username, u.email, u.password_hash AS PasswordHash,
               u.created_at AS CreatedAt, u.updated_at AS UpdatedAt, u.role_id AS RoleId
        FROM Roles r
        LEFT JOIN Users u ON r.role_id = u.role_id";  // Använd LEFT JOIN för att inkludera roller utan användare

            var roleDictionary = new Dictionary<int, Role>();

            var result = await connection.QueryAsync<Role, User, Role>(
                query,
                (role, user) =>
                {
                    if (!roleDictionary.TryGetValue(role.RoleId, out var currentRole))
                    {
                        currentRole = role;
                        currentRole.Users = new List<User>();
                        roleDictionary.Add(currentRole.RoleId, currentRole);
                    }

                    // Om användare finns, lägg till den till användarlistan för rollen
                    if (user != null && user.Id != 0) // Kontrollera att användaren har ett giltigt Id
                    {
                        currentRole.Users.Add(user);
                    }

                    return currentRole;
                },
                splitOn: "Id" // Ange "Id" som split-kolumnen för att separera användare från roller
            );

            return roleDictionary.Values;
        }



        // Hämta en roll efter ID
        public async Task<Role> FindById(int roleId)
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = @"
        SELECT r.role_id AS RoleId, r.Name, 
               u.Id AS Id, u.username, u.email, u.password_hash AS PasswordHash, 
               u.created_at AS CreatedAt, u.updated_at AS UpdatedAt, u.role_id AS RoleId
        FROM Roles r
        LEFT JOIN Users u ON r.role_id = u.role_id
        WHERE r.role_id = @RoleId";  // Använd LEFT JOIN för att inkludera roller utan användare

            var roleDictionary = new Dictionary<int, Role>();

            var result = await connection.QueryAsync<Role, User, Role>(
                query,
                (role, user) =>
                {
                    if (!roleDictionary.TryGetValue(role.RoleId, out var currentRole))
                    {
                        currentRole = role;
                        currentRole.Users = new List<User>();
                        roleDictionary.Add(currentRole.RoleId, currentRole);
                    }

                    // Om användare finns, lägg till den till användarlistan för rollen
                    if (user != null && user.Id != 0) // Kontrollera att användaren har ett giltigt Id
                    {
                        user.Role = currentRole;  // Sätt användarens "Role" till rollens namn
                        currentRole.Users.Add(user);
                    }

                    return currentRole;
                },
                new { RoleId = roleId },
                splitOn: "Id"  // Split för att separera User från Role
            );

            return roleDictionary.Values.FirstOrDefault(); // Returnera första (och enda) rollen
        }



        // Hämta en roll efter namn
        public async Task<Role> FindByName(string roleName)
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = "SELECT role_id AS RoleId, Name FROM Roles WHERE Name = @RoleName";
            return await connection.QueryFirstOrDefaultAsync<Role>(query, new { RoleName = roleName });
        }

        // Lägg till en ny roll
        public async Task<int> Add(Role role)
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = @"
                INSERT INTO Roles (Name)
                OUTPUT INSERTED.role_id
                VALUES (@Name)";
            return await connection.ExecuteScalarAsync<int>(query, new { role.Name });
        }

        // Uppdatera en roll
        public async Task<bool> Update(Role role)
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = @"
                UPDATE Roles
                SET Name = @Name
                WHERE role_id = @RoleId";
            int affectedRows = await connection.ExecuteAsync(query, role);
            return affectedRows > 0;
        }

        // Ta bort en roll
        public async Task<bool> Delete(int roleId)
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = "DELETE FROM Roles WHERE role_id = @RoleId";
            int affectedRows = await connection.ExecuteAsync(query, new { RoleId = roleId });
            return affectedRows > 0;
        }
    }
}