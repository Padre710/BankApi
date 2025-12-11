using BankApi.Models;
using System.Threading.Tasks;

namespace BankApi.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user);
        Task<User?> GetUserWithAccountsAsync(int userId);
    }
}

