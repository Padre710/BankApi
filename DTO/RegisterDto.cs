using BankApi.Models;
using System.ComponentModel.DataAnnotations;

namespace BankApi.DTO
{
    public class RegisterDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;  // unique
        public string Password { get; set; } = string.Empty; // store hashed passwords
        public Role Role { get; set; } 
    }
}
