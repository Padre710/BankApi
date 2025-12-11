using BankApi.DTO;
using BankApi.Models;
using System.Threading.Tasks;

namespace BankApi.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto dto);
        Task<(string token, User user)> LoginAsync(LoginDto dto);
        Task<User?> GetProfileAsync(int userId);
    }
}

