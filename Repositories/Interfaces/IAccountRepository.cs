using BankApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApi.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<IEnumerable<Account>> GetAllAsync();
        Task<Account?> GetByIdAsync(int id);
        Task<IEnumerable<Account>> GetByUserIdAsync(int userId);
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);
        Task DeleteAsync(Account account);
        Task<bool> AccountExistsAsync(string accountNumber);
        Task<IEnumerable<Account>> SearchByAccountNumberAsync(string accountNumber);
        Task SaveChangesAsync();
    }
}
