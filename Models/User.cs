using Microsoft.AspNetCore.Identity;

namespace BankApi.Models
{
    public enum Role
    {
        Admin,
        Teller,
        Customer
    }
    public class User 
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;   // unique
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public Role Role { get; set; }

        // Navigation: A Customer can own many  accounts
        public ICollection<Account> Accounts { get; set; }
    }
}
