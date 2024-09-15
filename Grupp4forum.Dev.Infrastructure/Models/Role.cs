using System.ComponentModel.DataAnnotations;

namespace Grupp4forum.Dev.Infrastructure.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();

    }
}
