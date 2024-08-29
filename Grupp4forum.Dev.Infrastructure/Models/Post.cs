using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grupp4forum.Dev.Infrastructure.Models
{
    public class Post
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }  
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }

        //// Navigationsegenskaper
        //public virtual User User { get; set; }
        //public virtual ICollection<Reply> Replies { get; set; }
    }
}
