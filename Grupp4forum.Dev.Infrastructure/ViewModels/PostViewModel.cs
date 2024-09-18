using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grupp4forum.Dev.Infrastructure.ViewModel
{
    public class PostViewModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string? Author { get; set; }
        public int? CategoryId { get; set; }
        public IFormFile? Image { get; set; }
    }
}
