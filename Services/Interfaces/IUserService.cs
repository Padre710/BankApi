using BankApi.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserReadDto>> GetAllUsersAsync(string role);
        Task<UserReadDto?> GetUserByIdAsync(int id);
        Task<string> UpdateUserAsync(int id, UserUpdateDto updatedUser, string role);
        Task<string> DeleteUserAsync(int id, string role);
        Task<IEnumerable<UserReadDto>> GetCustomersAsync();
        Task<IEnumerable<UserReadDto>> GetTellersAsync();
    }
}

