using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grupp4forum.Dev.Infrastructure.Models
{
    public class Category
    {
        public int CategoryId { get; set; }       // Primärnyckel
        public string Name { get; set; }          // Namn på kategorin
        public string Description { get; set; }   // Beskrivning av kategorin
        public DateTime CreatedAt { get; set; }   // Tidpunkt när kategorin skapades
        public DateTime UpdatedAt { get; set; }   // Tidpunkt när kategorin senast uppdaterades
    }
}

