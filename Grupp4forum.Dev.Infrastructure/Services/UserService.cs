using System.Collections.Generic;
using System.Threading.Tasks;
using Grupp4forum.Dev.Infrastructure.Models;
using Grupp4forum.Dev.Infrastructure.Repository;

namespace Grupp4forum.Dev.API.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private readonly RoleRepository _roleRepository;

        public UserService(UserRepository userRepository, RoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        // Hämta alla användare
        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _userRepository.FindAllWithRoles();
        }

        // Hämta en användare efter ID
        public async Task<User> GetUserById(int id)
        {
            return await _userRepository.FindByIdWithRole(id);
        }

        // Lägg till en ny användare
        public async Task<int> AddUser(User user)
        {
            // Hämta standardrollen "User"
            var standardRole = await _roleRepository.FindByName("User");

            if (standardRole == null)
            {
                throw new InvalidOperationException("Standardrollen 'User' kunde inte hittas.");
            }

            // Tilldela standardrollen till användaren
            user.RoleId = standardRole.RoleId;  // Använd "role_id"

            // Lägg till användaren i databasen
            return await _userRepository.Add(user);
        }

        // Uppdatera en användare
        public async Task<bool> UpdateUser(User user)
        {
            return await _userRepository.Update(user);
        }

        // Ta bort en användare
        public async Task<bool> DeleteUser(int id, int userId)
        {
            return await _userRepository.Delete(id, userId);
        }

        // Lägg till en roll till en användare
        public async Task<bool> AddRoleToUser(int userId, int roleId)
        {
            var user = await _userRepository.FindByIdWithRole(userId);
            var role = await _roleRepository.FindById(roleId);

            if (user == null || role == null)
            {
                return false;
            }

            // Uppdatera användarens roll till den nya rollen
            user.RoleId = role.RoleId;  // Använd "role_id"

            return await _userRepository.Update(user);
        }

        // Hitta användare baserat på användarnamn
        public async Task<User> FindUserByUsername(string username)
        {
            return await _userRepository.FindByUsername(username);
        }

        public async Task<bool> IsAdminOrModerator(int userId)
        {
            return await _userRepository.IsAdminOrModerator(userId);
        }

    }
}
