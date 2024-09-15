using System.Collections.Generic;
using System.Threading.Tasks;
using Grupp4forum.Dev.Infrastructure.Models;
using Grupp4forum.Dev.Infrastructure.ViewModel;
using Grupp4forum.Dev.Infrastructure.Repository;

namespace Grupp4forum.Dev.API.Services
{
    public class RoleService
    {
        private readonly RoleRepository _roleRepository;

        public RoleService(RoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        // Metod för att hämta alla roller
        public async Task<IEnumerable<Role>> GetAllRoles()
        {
            return await _roleRepository.GetAllRoles();
        }

        // Metod för att hämta en roll efter ID
        public async Task<Role> GetRoleById(int id)
        {
            return await _roleRepository.FindById(id);
        }

        // Metod för att hämta en roll efter namn
        public async Task<Role> GetRoleByName(string name)
        {
            return await _roleRepository.FindByName(name);
        }

        // Metod för att lägga till en ny roll
        public async Task<int> AddRole(RoleViewModel roleViewModel)
        {
            // Konvertera från ViewModel till Model
            var role = new Role
            {
                Name = roleViewModel.Name
            };

            return await _roleRepository.Add(role);
        }

        // Metod för att uppdatera en roll
        public async Task<bool> UpdateRole(RoleViewModel roleViewModel)
        {
            // Konvertera från ViewModel till Model
            var role = new Role
            {
                RoleId = roleViewModel.RoleId,
                Name = roleViewModel.Name
            };

            return await _roleRepository.Update(role);
        }

        // Metod för att ta bort en roll
        public async Task<bool> DeleteRole(int roleId)
        {
            return await _roleRepository.Delete(roleId);
        }
    }
}
