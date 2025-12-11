using BankApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync(string role);
        Task<User?> GetUserByIdAsync(int id);
        Task<string> UpdateUserAsync(int id, User updatedUser, string role);
        Task<string> DeleteUserAsync(int id, string role);
        Task<IEnumerable<User>> GetCustomersAsync();
        Task<IEnumerable<User>> GetTellersAsync();
    }
}

