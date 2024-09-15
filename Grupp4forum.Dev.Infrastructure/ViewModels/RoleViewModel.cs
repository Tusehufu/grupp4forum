using Grupp4forum.Dev.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grupp4forum.Dev.Infrastructure.ViewModel
{
    public class RoleViewModel
    {
        public int RoleId { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

    }
}
