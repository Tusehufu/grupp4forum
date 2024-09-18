using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grupp4forum.Dev.Infrastructure.Models
{
    public class ReplyLike
    {
        public int Id { get; set; }
        public int ReplyId { get; set; }
        public int UserId { get; set; }
    }
}
