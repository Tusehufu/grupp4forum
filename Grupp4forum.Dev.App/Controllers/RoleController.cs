using System.Collections.Generic;
using System.Threading.Tasks;
using Grupp4forum.Dev.Infrastructure.Models;
using Grupp4forum.Dev.Infrastructure.ViewModel;
using Grupp4forum.Dev.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grupp4forum.Dev.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly RoleService _roleService;

        public RoleController(RoleService roleService)
        {
            _roleService = roleService;
        }

        // GET: api/roles
        [HttpGet]
        public async Task<IEnumerable<Role>> GetAll()
        {
            return await _roleService.GetAllRoles();
        }

        // GET: api/roles/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _roleService.GetRoleById(id);

            if (role == null)
            {
                return NotFound();
            }

            return Ok(role);
        }

        // POST: api/roles
        [HttpPost]
        public async Task<IActionResult> Create(RoleViewModel roleViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newRoleId = await _roleService.AddRole(roleViewModel);
            return CreatedAtAction(nameof(GetById), new { id = newRoleId }, roleViewModel);
        }

        // PUT: api/roles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RoleViewModel roleViewModel)
        {
            if (id != roleViewModel.RoleId || !ModelState.IsValid)
            {
                return BadRequest("Invalid role data.");
            }

            var updated = await _roleService.UpdateRole(roleViewModel);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/roles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _roleService.DeleteRole(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
