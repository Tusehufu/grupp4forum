using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grupp4forum.Dev.Infrastructure.Models
{
    public class Reply
    {
        public int ReplyId { get; set; }

        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public int ParentReplyId { get; set; }
        public int Likes { get; set; }
        public byte[]? Image { get; set; }
        public string? ImageBase64 { get; set; } // Base64-sträng för frontend
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public bool IsVisible { get; set; }


    }
}
