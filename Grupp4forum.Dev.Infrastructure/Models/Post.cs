using System;

namespace Grupp4forum.Dev.Infrastructure.Models
{
    public class Post
    {
        public int PostId { get; set; }          // Primärnyckel
        public int UserId { get; set; }          // Referens till användaren som skapade inlägget
        public string Title { get; set; }        // Titel för inlägget
        public string Author { get; set; }       // Författaren (baserat på användarens username)
        public string Content { get; set; }      // Innehållet i inlägget
        public int? CategoryId { get; set; }     // Referens till kategori (nullable)
        public bool IsVisible { get; set; }
        public int Likes { get; set; }
        public DateTime CreatedAt { get; set; }  // Tidpunkt när inlägget skapades
        public DateTime UpdatedAt { get; set; }  // Tidpunkt när inlägget senast uppdaterades

        //// Navigationsegenskaper
        //public virtual User User { get; set; }   // Referens till användaren (kan kommenteras bort om det inte används)
        //public virtual Category Category { get; set; } // Referens till kategori
        //public virtual ICollection<Reply> Replies { get; set; }  // Samling av replies för inlägget (om det används)
    }
}
