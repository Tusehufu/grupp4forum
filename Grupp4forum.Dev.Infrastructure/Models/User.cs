using Grupp4forum.Dev.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Grupp4forum.Dev.Infrastructure.Models
{
    public class User
    {
        public int Id { get; set; }                // Primärnyckel
        public string Username { get; set; }       // Användarnamn

        [NotMapped]
        [JsonIgnore]
        public string Password { get; set; }       //Lösenord

        public string Email { get; set; }          // E-postadress
        public string PasswordHash { get; set; }   // Lösenord (lagrat som hash)
        public DateTime CreatedAt { get; set; }    // När användaren skapades
        public DateTime UpdatedAt { get; set; }    // När användaren senast uppdaterades
        public int RoleId { get; set; }            // Rollens ID

        [JsonIgnore]
        // Navigeringsegenskap till rollen
        public Role Role { get; set; }             // Referens till `Role`-klassen (för relationen med `roles`-tabellen)
    }
}