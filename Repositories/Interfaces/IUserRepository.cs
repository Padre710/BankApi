using BankApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApi.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(User user);
        Task<IEnumerable<User>> GetCustomersAsync();
        Task<IEnumerable<User>> GetTellersAsync();
    }
}
